using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net;
using SharedCode.Model;
using System.Net.Http;
using Newtonsoft.Json;
using SharedCodeLibrary.HttpCommand;

public class InGameSession
{
    public long SessionId { get; private set; }
    public IPEndPoint GameRoomEndPoint { get; private set; }
    public List<PlayerInfo> Users; // 접속한 플레이어 관리

    private Socket listenSocket;
    private bool isRunning;
    private Thread sessionThread;
    private SemaphoreSlim maxConnectionsSemaphore = new SemaphoreSlim(2); // 세션 당 최대 연결 수
    private ConcurrentQueue<SocketAsyncEventArgs> eventArgsPool = new ConcurrentQueue<SocketAsyncEventArgs>();
    private ManualResetEvent sessionEndedEvent = new ManualResetEvent(false);
    private InGameWorld world; // 인게임 세계 객체
    private SessionInfoManager sessionInfoMng;
    //private SessionManager sessionManager;

    public InGameSession() // List<long>로 변경
    {
        SessionId = DateTime.Now.Ticks;
        Users = new List<PlayerInfo>();
        world = new InGameWorld(this);
        sessionInfoMng = new SessionInfoManager(this);
        listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        // SocketAsyncEventArgs 초기화
        for (int i = 0; i < 100; i++)
        {
            SocketAsyncEventArgs eventArg = new SocketAsyncEventArgs();
            eventArg.Completed += IO_Completed;
            byte[] buffer = new byte[4096];
            eventArg.SetBuffer(buffer, 0, buffer.Length);
            eventArgsPool.Enqueue(eventArg);
        }
    }


    public void StartSession()
    {
        isRunning = true;
        sessionEndedEvent.Reset();

        // 모든 네트워크 인터페이스에서 수신 대기
        string localIP = GetLocalIPAddress(); // 로컬 IP 주소로 설정
        listenSocket.Bind(new IPEndPoint(IPAddress.Parse(localIP), 0)); // 특정 IP 주소와 임의의 포트 사용
        listenSocket.Listen(100);
        if (listenSocket.LocalEndPoint is not IPEndPoint gameRoomEndPoint)
        {
            throw new Exception("Listen socket or local endpoint is null.");
        }
        GameRoomEndPoint = gameRoomEndPoint;

        Logger.SetLogger(LOGTYPE.INFO, $"Session {SessionId} started on IP {localIP}, port {GameRoomEndPoint.Port}");
        Logger.SetLogger(LOGTYPE.INFO, $"Listening on {listenSocket.LocalEndPoint.ToString()}");

        SendIPEndPointToMainServer(GameRoomEndPoint);

        // 리스닝 및 업데이트를 하나의 쓰레드에서 처리
        sessionThread = new Thread(RunSession);
        sessionThread.Start();
    }
    private async void SendIPEndPointToMainServer(IPEndPoint endPoint)
    {
        using (HttpClient client = new HttpClient())
        {
            var url = "http://127.0.0.1:5000/InGameSession/CreatedSessionIPEndPointSendToClient";

            // IPEndPoint 정보를 JSON으로 직렬화
            NewInGameSessionRequest requestData = new NewInGameSessionRequest
            {
                SessionId = this.SessionId.ToString(),
                IPandPort = $"{endPoint.Address}:{endPoint.Port}"
            };
            // 객체를 JSON으로 직렬화
            var jsonContent = new StringContent(JsonConvert.SerializeObject(requestData), System.Text.Encoding.UTF8, "application/json");

            try
            {
                // POST 요청 전송
                HttpResponseMessage response = await client.PostAsync(url, jsonContent);
                response.EnsureSuccessStatusCode(); // 성공적으로 응답을 받지 못하면 예외 발생

                Logger.SetLogger(LOGTYPE.INFO, $"Successfully sent IPEndPoint to server: {requestData.IPandPort}");
            }
            catch (Exception ex)
            {
                Logger.SetLogger(LOGTYPE.ERROR, $"Failed to send IPEndPoint to server: {ex.Message}");
            }
        }
    }
    public void StopSession()
    {
        isRunning = false;
        sessionEndedEvent.Set();
        Logger.SetLogger(LOGTYPE.INFO, $"Session {SessionId} stopped.");
        sessionThread.Join(); // 스레드 종료 대기
    }

    public void AddPlayer(PlayerInfo player)
    {
        Users.Add(player);

        InGamePlayerInfo inGamePlayerInfo = new InGamePlayerInfo();
        inGamePlayerInfo.UserUID = player.UserUID;
        inGamePlayerInfo.PlayerNumber = sessionInfoMng.InGamePlayerInfos.Count + 1;
        sessionInfoMng.InGamePlayerInfos.Add(inGamePlayerInfo);

        Character character = new Character();
        character.Uid = player.UserUID;
        character.PlayerNum = inGamePlayerInfo.PlayerNumber;
        world.UsersCharacter.Add(character);

        // Logger.SetLogger(LOGTYPE.INFO$"Player {player.UserUID} added to session {SessionId}");
    }
    public async Task AddPlayerAsync(PlayerInfo player)
    {
        Users.Add(player);

        InGamePlayerInfo inGamePlayerInfo = new InGamePlayerInfo
        {
            UserUID = player.UserUID,
            PlayerNumber = sessionInfoMng.InGamePlayerInfos.Count + 1
        };
        sessionInfoMng.InGamePlayerInfos.Add(inGamePlayerInfo);

        Character character = new Character
        {
            Uid = player.UserUID,
            PlayerNum = inGamePlayerInfo.PlayerNumber
        };
        world.UsersCharacter.Add(character);

        await Task.CompletedTask; // 실제 비동기 작업으로 대체
    }
    public void SetSocket(PlayerInfo player)
    {
        // 클라이언트 소켓을 인게임 세션으로 넘기고, ReceiveAsync를 호출하여 인게임 세션에서 패킷을 받도록 함
        SocketAsyncEventArgs receiveEventArg = new SocketAsyncEventArgs();
        if (player.Socket == null) return;

        receiveEventArg.UserToken = player.Socket;
        receiveEventArg.SetBuffer(new byte[1024], 0, 1024);
        receiveEventArg.Completed += IO_Completed;
        if (!player.Socket.ReceiveAsync(receiveEventArg))
        {
            IO_Completed(this, receiveEventArg);
        }
    }

    private void RunSession()
    {
        // 비동기 소켓 연결 수락 시작
        Accept();


        // 20Hz 업데이트 타이머
        Timer updateTimer = new Timer(UpdateGameWorld, null, 0, 25); // 50ms 간격으로 업데이트 (20Hz)

        // 세션이 종료될 때까지 대기
        sessionEndedEvent.WaitOne();

        // 세션 종료 시 타이머 정리
        updateTimer.Dispose();
    }

    private void Accept()
    {
        SocketAsyncEventArgs acceptEventArg = new SocketAsyncEventArgs();
        acceptEventArg.Completed += AcceptCompleted;

        if (listenSocket == null)
        {
            return;
        }

        if (!listenSocket.AcceptAsync(acceptEventArg))
        {
            AcceptCompleted(this, acceptEventArg);
        }
    }

    private void AcceptCompleted(object sender, SocketAsyncEventArgs e)
    {
        if (e.SocketError == SocketError.Success)
        {
            if (e.AcceptSocket == null)
                return;
            Socket clientSocket = e.AcceptSocket;
            Logger.SetLogger(LOGTYPE.INFO, $"Client connected to session {SessionId}: {clientSocket.RemoteEndPoint}");

            maxConnectionsSemaphore.Wait();

            if (eventArgsPool == null)
                return;
            if (eventArgsPool.TryDequeue(out SocketAsyncEventArgs receiveEventArg))
            {
                receiveEventArg.UserToken = clientSocket;

                if (!clientSocket.ReceiveAsync(receiveEventArg))
                {
                    IO_Completed(this, receiveEventArg);
                }
            }
        }
        else
        {
            Logger.SetLogger(LOGTYPE.INFO, $"Error accepting client: {e.SocketError}");
        }

        e.AcceptSocket = null;
        if (isRunning)
        {
            Accept();
        }
    }

    public void IO_Completed(object sender, SocketAsyncEventArgs e)
    {
        if (e.UserToken == null)
            return;
        Socket clientSocket = (Socket)e.UserToken;

        if (clientSocket == null || !clientSocket.Connected)
        {
            // 소켓이 null이거나 연결이 끊어졌을 때
            RemoveClient(clientSocket);
            ReleaseEventArgs(e);
            return;
        }

        if (e.LastOperation == SocketAsyncOperation.Receive)
        {
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                if (e.Buffer == null)
                {
                    return;
                }

                HandlePacket(clientSocket, e.Buffer, e.Offset, e.BytesTransferred);

                if (!clientSocket.ReceiveAsync(e))
                {
                    IO_Completed(this, e);
                }
            }
            else
            {
                RemoveClient(clientSocket);
                ReleaseEventArgs(e);
            }
        }
        else if (e.LastOperation == SocketAsyncOperation.Send)
        {
            if (e.SocketError == SocketError.Success)
            {
                // 전송 성공 후 추가 작업 가능

                // 이벤트 인스턴스를 다시 풀에 반환
                ReleaseEventArgs(e);
            }
            else
            {
                // 전송 실패 처리
                RemoveClient(clientSocket);
                ReleaseEventArgs(e);
            }
        }
    }

    public void HandlePacket(Socket clientSocket, byte[] buffer, int offset, int count)
    {
        byte protocol = buffer[0];
        byte[] lengthBytes = new byte[4];

        try
        {
            for (int i = 0; i < 4; i++)
            {
                lengthBytes[i] = buffer[i + 1];
            }
        }
        catch (Exception ex)
        {
            Logger.SetLogger(LOGTYPE.ERROR, ex.Message);
        }

        int length = BitConverter.ToInt32(lengthBytes, 0);
        count -= 5;
        byte[] realData = new byte[length];

        try
        {
            for (int i = 0; i < count; i++)
            {
                realData[i] = buffer[i + 5];
            }
        }
        catch (Exception ex)
        {
            Logger.SetLogger(LOGTYPE.ERROR, ex.Message);
        }

        switch (protocol)
        {
            case (byte)InGameProtocol.SessionInfo:
                sessionInfoMng.ProcessSessionInfoPacket(realData, clientSocket);
                break;
            case (byte)InGameProtocol.CharacterTr:
                world.UpdatePlayerTR(realData);
                break;
            case (byte)InGameProtocol.GameInfo:
                world.UpdatePlayerDataInfo(realData);
                break;
            default:
                break;
        }
    }

    private async void RemoveClient(Socket clientSocket)
    {
        if (clientSocket == null)
        {
            return;
        }

        clientSocket.Close();

        Users.RemoveAll(user => user.Socket == clientSocket);

        if (Users.Count <= 0)//만약 접속자가 0명이면 일단 지금은 테스트를 위해서 사람이없으니 세션을 정리해버리자
        {
            //await sessionManager.RemoveSession(this.SessionId);
        }
    }

    private void ReleaseEventArgs(SocketAsyncEventArgs e)
    {
        e.UserToken = null;
        maxConnectionsSemaphore.Release();
        eventArgsPool.Enqueue(e);
    }

    private void UpdateGameWorld(object state)
    {
        UpdateCharacterTR();
    }

    private void UpdateCharacterTR()
    {
        if (sessionInfoMng.IsAllPlayerReady)
        {
            foreach (var user in world.UsersCharacter)
            {
                Packet characterTR = new Packet();

                int length = 0x01 + Utils.GetLength(user.Uid) + Utils.GetLength(user.PlayerNum) +
                    Utils.GetLength(user.Position.X) + Utils.GetLength(user.Position.Y) + Utils.GetLength(user.Position.Z) +
                    Utils.GetLength(user.Quaternion.X) + Utils.GetLength(user.Quaternion.Y) + Utils.GetLength(user.Quaternion.Z) + Utils.GetLength(user.Quaternion.W);
                characterTR.push((byte)InGameProtocol.CharacterTr);
                characterTR.push(length);
                characterTR.push((byte)SessionInfo.TransformInfo);
                characterTR.push(user.Uid);
                characterTR.push(user.PlayerNum);
                characterTR.push(user.Position.X);
                characterTR.push(user.Position.Y);
                characterTR.push(user.Position.Z);
                characterTR.push(user.Quaternion.X);
                characterTR.push(user.Quaternion.Y);
                characterTR.push(user.Quaternion.Z);
                characterTR.push(user.Quaternion.W);
                SendToAllClient(characterTR);

                // Logger.SetLogger(LOGTYPE.INFO, $"User {user.uid} Position: X={user.m_position.X}, Y={user.m_position.Y}, Z={user.m_position.Z}");
            }
        }
    }

    private string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }

    public void SendToClient(Socket clientSocket, Packet packet)
    {
        if (clientSocket == null)
            return;

        if (eventArgsPool == null) return;

        if (eventArgsPool.TryDequeue(out SocketAsyncEventArgs sendEventArg))
        {
            sendEventArg.SetBuffer(packet.Buffer, 0, packet.Position);
            sendEventArg.UserToken = clientSocket;

            if (!clientSocket.SendAsync(sendEventArg))
            {
                IO_Completed(this, sendEventArg);
            }
        }
    }

    public void SendToAllClient(Packet packet)
    {
        if (Users == null) return;

        foreach (var user in Users)
        {
            if (user.Socket == null) continue;
            SendToClient(user.Socket, packet);
        }
    }

}
