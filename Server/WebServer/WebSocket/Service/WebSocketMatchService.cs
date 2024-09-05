using Newtonsoft.Json;
using SharedCode.Model;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.WebSockets;
using WebServer.Repository.Interface;

public class WebSocketMatchService
{
    private RatingRange ratingRange = new RatingRange();
    private Queue<PlayerInfo> normalQueue = new Queue<PlayerInfo>();
    private Queue<List<PlayerInfo>> waitQueue = new Queue<List<PlayerInfo>>();
    private Dictionary<Tier, Queue<PlayerInfo>> rankQueues;
    private const int MatchSize = 1; // ��Ī�� �ʿ��� �ּ� �÷��̾� �� (��: 2��)
    private Dictionary<long, List<PlayerInfo>> pendingMatches = new Dictionary<long, List<PlayerInfo>>();
    private Dictionary<long, GameType> matchTypes = new Dictionary<long, GameType>();

    private readonly IAccountRepository _accountRepository;
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private readonly SemaphoreSlim _matchSemaphore = new SemaphoreSlim(1, 1);

    // �� ������ �����ϴ� ���μ����� �����մϴ�.
    private ConcurrentDictionary<long, InGameSessionInfo> inGameSessionInfos = new ConcurrentDictionary<long, InGameSessionInfo>();

    public WebSocketMatchService(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
        Logger.SetLogger(LOGTYPE.INFO, $"{this.ToString()} init Complete");
        rankQueues = new Dictionary<Tier, Queue<PlayerInfo>>();
        foreach (Tier tier in Enum.GetValues(typeof(Tier)))
        {
            rankQueues[tier] = new Queue<PlayerInfo>();
        }
        Task.Run(() => MatchMakingLoopAsync(_cancellationTokenSource.Token));
    }

    public async Task ProcessMatchPacket(WebSocket webSocket, byte[] data)
    {
        if (data[0] == (byte)MatchProtocol.MatchStart)
        {
            if (data[1] == (byte)GameType.Normal)
            {
                byte[] UserUid = data.Skip(2).ToArray();
                await InsertUserNormalQueue(webSocket, UserUid);
            }
            else if (data[1] == (byte)GameType.Rank)
            {
                byte[] UserUid = data.Skip(2).ToArray();
                await InsertUserRankQueue(webSocket, UserUid);
            }
        }
        else if (data[0] == (byte)MatchProtocol.MatchStop)
        {
            if (data[1] == (byte)GameType.Normal)
            {
                byte[] UserUid = data.Skip(2).ToArray();
                await DeleteUserNormalQueue(webSocket, UserUid);
            }
            else if (data[1] == (byte)GameType.Rank)
            {
                byte[] UserUid = data.Skip(2).ToArray();
                await DeleteUserRankQueue(webSocket, UserUid);
            }
        }
        else if (data[0] == (byte)MatchProtocol.GameAccept)
        {
            byte[] UserUid = data.Skip(1).ToArray();
            await HandleGameAccept(webSocket, BitConverter.ToInt64(UserUid));
        }
    }

    private async Task MatchMakingLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await TryMatchNormalQueue();
            await TryMatchRankQueue();
            await Task.Delay(50);
        }
    }
    int i = 0;
    private async Task HandleGameAccept(WebSocket clientSocket, long userUid)
    {
        foreach (var match in pendingMatches)
        {
            var players = match.Value;
            var player = players.FirstOrDefault(p => p.UserUID == userUid);
            if (player != null)
            {
                player.HasAccepted = true;

                // ��� �÷��̾ �����ߴ��� Ȯ��
                if (players.All(p => p.HasAccepted))
                {
                    // RunProcess�� ȣ���Ͽ� �ΰ��� ���� ���μ��� ����
                    await RunProcess();
                    waitQueue.Enqueue(players);
                    Logger.SetLogger(LOGTYPE.DEBUG, i++);
                    // ��� ���·� �����ϰ�, �ΰ��� ������ HTTP ��û���� ���� ������ ���� ������ ���
                    pendingMatches.Remove(match.Key);
                    matchTypes.Remove(match.Key);
                }
                break;
            }
        }
    }

    private Task DeleteUserNormalQueue(WebSocket clientSocket, byte[] data)
    {
        long userUID = BitConverter.ToInt64(data, 0);

        lock (normalQueue)
        {
            var tempQueue = new Queue<PlayerInfo>();
            while (normalQueue.Count > 0)
            {
                var playerInfo = normalQueue.Dequeue();
                if (playerInfo.UserUID != userUID)
                {
                    tempQueue.Enqueue(playerInfo);
                }
                else
                {
                    Logger.SetLogger(LOGTYPE.INFO, $"User {userUID} removed from normal queue.");
                }
            }

            normalQueue = tempQueue;
        }

        return Task.CompletedTask;
    }

    private async Task InsertUserNormalQueue(WebSocket clientSocket, byte[] data)
    {
        long userUID = BitConverter.ToInt64(data, 0);

        var userRating = await _accountRepository.GetMyRating(userUID);

        PlayerInfo playerInfo = new PlayerInfo
        {
            UserUID = userUID,
            Rating = userRating.Item2,
            WebSocket = clientSocket
        };

        lock (normalQueue)
        {
            normalQueue.Enqueue(playerInfo);
            Logger.SetLogger(LOGTYPE.INFO, $"User {userUID} added to normal queue.");
        }
    }

    public async Task TryMatchNormalQueue()
    {
        await _matchSemaphore.WaitAsync();
        try
        {
            if (normalQueue.Count >= MatchSize)
            {
                long matchId = DateTime.Now.Ticks;
                pendingMatches[matchId] = new List<PlayerInfo>();

                for (int i = 0; i < MatchSize; i++)
                {
                    pendingMatches[matchId].Add(normalQueue.Dequeue());
                }

                matchTypes[matchId] = GameType.Normal;

                Logger.SetLogger(LOGTYPE.INFO, "Match found:");
                foreach (var player in pendingMatches[matchId])
                {
                    Logger.SetLogger(LOGTYPE.INFO, $"User {player.UserUID} matched.");
                }

                // ��Ī�� �÷��̾�鿡�� ������ ������ ���� ���� ���� ���
                var sendTasks = pendingMatches[matchId].Select(player => SendMatchResponse(player.WebSocket));
                await Task.WhenAll(sendTasks);
            }
        }
        finally
        {
            _matchSemaphore.Release();
        }
    }

    private async Task InsertUserRankQueue(WebSocket clientSocket, byte[] data)
    {
        long userUID = BitConverter.ToInt64(data, 0);

        var playerRating = await _accountRepository.GetMyRating(userUID);

        PlayerInfo playerInfo = new PlayerInfo
        {
            UserUID = userUID,
            Rating = playerRating.Item2,
            WebSocket = clientSocket
        };

        Tier tier = RatingRange.GetTier(playerInfo.Rating);
        lock (rankQueues[tier])
        {
            rankQueues[tier].Enqueue(playerInfo);
        }

        Logger.SetLogger(LOGTYPE.INFO, $"User {userUID} added to {tier} rank queue.");
    }

    private Task DeleteUserRankQueue(WebSocket clientSocket, byte[] data)
    {
        long userUID = BitConverter.ToInt64(data, 0);

        foreach (var queue in rankQueues.Values)
        {
            lock (queue)
            {
                var tempQueue = new Queue<PlayerInfo>();
                while (queue.Count > 0)
                {
                    var playerInfo = queue.Dequeue();
                    if (playerInfo.UserUID != userUID)
                    {
                        tempQueue.Enqueue(playerInfo);
                    }
                    else
                    {
                        Logger.SetLogger(LOGTYPE.INFO, $"User {userUID} removed from rank queue.");
                    }
                }

                while (tempQueue.Count > 0)
                {
                    queue.Enqueue(tempQueue.Dequeue());
                }
            }
        }

        return Task.CompletedTask;
    }

    public async Task TryMatchRankQueue()
    {
        foreach (var tierQueue in rankQueues)
        {
            await _matchSemaphore.WaitAsync();
            try
            {
                if (tierQueue.Value.Count >= MatchSize)
                {
                    long matchId = DateTime.Now.Ticks;
                    pendingMatches[matchId] = new List<PlayerInfo>();

                    for (int i = 0; i < MatchSize; i++)
                    {
                        pendingMatches[matchId].Add(tierQueue.Value.Dequeue());
                    }

                    matchTypes[matchId] = GameType.Rank;

                    Logger.SetLogger(LOGTYPE.INFO, $"Match found in {tierQueue.Key} tier:");
                    foreach (var player in pendingMatches[matchId])
                    {
                        Logger.SetLogger(LOGTYPE.INFO, $"User {player.UserUID} matched.");
                    }

                    // ��Ī�� �÷��̾�鿡�� ������ ����
                    var sendTasks = pendingMatches[matchId].Select(player => SendMatchResponse(player.WebSocket));
                    await Task.WhenAll(sendTasks);
                }
            }
            finally
            {
                _matchSemaphore.Release();
            }
        }
    }

    private async Task SendMatchResponse(WebSocket clientSocket)
    {
        Packet packet = new Packet();

        int length = 0x01;

        packet.push((byte)Protocol.Match);
        packet.push(length);
        packet.push((byte)MatchProtocol.GameMatched);

        await ClientManager.SendToSocket(clientSocket, packet);
    }

    // �ΰ��� ���� ���μ��� ���� (RunProcess)
    public async Task RunProcess()
    {
        // ���� ���μ��� ���� (�ΰ��� ���� ���μ��� ����)
        string exePath = @"D:\PortPolio\InGameServer\InGameServer\InGameServer\bin\Debug\net8.0\InGameServer.exe";

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = exePath,
            UseShellExecute = true,
            CreateNoWindow = false // ���μ��� ���ุ �� (���� ���� ����)
        };

        await Logger.SetLoggerAsync(LOGTYPE.INFO,"Session Process Start");

        Process.Start(startInfo);
    }

    // �ΰ��� �������� ���� IP�� Port ������ ��� ���� �÷��̾�鿡�� ����
    public async Task<bool> SendSessionIPAndPortToClient(long sessionId, IPEndPoint iPandPort)
    {
        InGameSessionInfo inGamePlayerInfo = new InGameSessionInfo(sessionId);
        inGamePlayerInfo.playerInfos = waitQueue.Dequeue();

        inGamePlayerInfo.GameRoomEndPoint = iPandPort;

        inGameSessionInfos.TryAdd(sessionId, inGamePlayerInfo);

        Logger.SetLogger(LOGTYPE.INFO, $"session : {sessionId} IP & Port : {iPandPort.Address}:{iPandPort.Port} is ready");

        if (inGameSessionInfos.TryGetValue(sessionId, out InGameSessionInfo _inGameSessionInfo))
        {
            foreach (var playerInfo in _inGameSessionInfo.playerInfos)
            {
                await SendGameRoomIP(playerInfo.WebSocket, iPandPort);
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    public InGameSessionInfo GetSessionProcess(long sessionId)
    {
        if (inGameSessionInfos.TryGetValue(sessionId, out InGameSessionInfo _inGameSessionInfo))
        {
            return _inGameSessionInfo;
        }
        return null;
    }

    private async Task SendGameRoomIP(WebSocket clientSocket, IPEndPoint gameRoomEndPoint)
    {
        Packet packet = new Packet();

        int length = 0x01 + 0x04 + 0x04;

        packet.push((byte)Protocol.Match);
        packet.push(length); // Protocol byte + IP address length (4 bytes) + port length (2 bytes)
        packet.push((byte)MatchProtocol.GameRoomIP);

        byte[] ipBytes = gameRoomEndPoint.Address.GetAddressBytes();
        packet.push(ipBytes);

        byte[] portBytes = BitConverter.GetBytes(gameRoomEndPoint.Port); // Port�� 2 bytes
        packet.push(gameRoomEndPoint.Port);

        await ClientManager.SendToSocket(clientSocket, packet);
    }
}
