using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using MessagePack;
using SharedCode.MessagePack;

public class InGameTCPController : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;
    private Thread receiveThread;
    private const int bufferSize = 4096;

    private static InGameTCPController instance;
    public static InGameTCPController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<InGameTCPController>();
                if (instance == null)
                {
                    GameObject singletonObject = new GameObject("InGameTCPControllerSingleton");
                    instance = singletonObject.AddComponent<InGameTCPController>();
                    var singletonParent = FindObjectOfType<InGameSingleton>();
                    Instantiate(singletonObject, singletonParent.transform);
                }
            }
            return instance;
        }
    }

    public void Init(string IP, int port)
    {
        Debug.Log("InGameTCPController Init Complete");
        ConnectToServer(IP, port); // 서버 IP 주소 및 포트
    }

    void OnApplicationQuit()
    {
        // 애플리케이션 종료 시 클라이언트 종료
        Disconnect();
    }

    public void ConnectToServer(string serverIp, int serverPort)
    {
        try
        {
            Debug.Log($"Trying to connect to server at {serverIp}:{serverPort}");
            client = new TcpClient(serverIp, serverPort);
            stream = client.GetStream();
            receiveThread = new Thread(new ThreadStart(ReceivePackets));
            receiveThread.Start();

            Debug.Log("Connected to server.");

            var packet = new Packet
            {
                Protocol = (byte)InGameProtocol.SessionInfo,
                Data = MessagePackSerializer.Serialize(new SessionSyncPacket
                {
                    SessionInfo = SessionInfo.SessionSyncOK,
                    UserUID = Global.Instance.standbyInfo.userEntity.UserUID
                })
            };

            SendToInGameServer(packet);
        }
        catch (Exception e)
        {
            Debug.LogError($"Connection error: {e.Message}");
        }
    }

    public void Disconnect()
    {
        if (receiveThread != null) receiveThread.Abort();
        if (stream != null) stream.Close();
        if (client != null) client.Close();
        Debug.Log("Disconnected from server.");
    }

    private void ReceivePackets()
    {
        Global.Instance.StaticLog("StartInGame ReadThread");

        byte[] buffer = new byte[bufferSize];

        try
        {
            while (true)
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    byte[] receivedData = new byte[bytesRead];
                    Array.Copy(buffer, receivedData, bytesRead);

                    EnqueueDispatcher(() => HandlePacket(receivedData));
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Receive error: {e.Message}");
        }
    }

    private void HandlePacket(byte[] buffer)
    {
        var packet = MessagePackSerializer.Deserialize<Packet>(buffer);

        switch (packet.Protocol)
        {
            case (byte)InGameProtocol.CharacterTr:
                CharacterTrController.Instance.ProcessUpdatePlayerPacket(packet.Data);
                break;
            case (byte)InGameProtocol.SessionInfo:
                InGameSessionController.Instance.ProcessSessionPacket(packet.Data);
                break;
            case (byte)InGameProtocol.GameInfo:
                InGameInfoManager.Instance.ProcessInGameInfoPacket(packet.Data);
                break;
            default:
                Debug.LogWarning("Unknown protocol received.");
                break;
        }
    }

    public void SendToInGameServer<T>(T data)
    {
        try
        {
            if (stream != null)
            {
                byte[] serializedData = MessagePackSerializer.Serialize(data);
                stream.Write(serializedData, 0, serializedData.Length);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Send error: {e.Message}");
        }
    }

    private static readonly Queue<Action> ExecutionQueue = new Queue<Action>();

    // 메인 스레드에서 작업을 예약하는 메서드
    public void EnqueueDispatcher(Action action)
    {
        lock (ExecutionQueue)
        {
            ExecutionQueue.Enqueue(action);
        }
    }

    // 매 프레임마다 큐에 있는 모든 작업을 실행하는 메서드
    public void ExecutePending()
    {
        lock (ExecutionQueue)
        {
            while (ExecutionQueue.Count > 0)
            {
                ExecutionQueue.Dequeue().Invoke();
            }
        }
    }

    private void Update()
    {
        ExecutePending();
    }
}
