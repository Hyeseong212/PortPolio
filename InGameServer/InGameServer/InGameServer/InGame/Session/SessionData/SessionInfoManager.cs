using SharedCode.Model;
using System.Net.Sockets;

internal class SessionInfoManager
{
    public InGameSession InGameSession;
    public bool IsAllPlayerReady;
    public List<InGamePlayerInfo> InGamePlayerInfos;
    public SessionInfoManager(InGameSession inGameSession)
    {
        InGameSession = inGameSession;
        InGamePlayerInfos = new List<InGamePlayerInfo>();
    }
    public void ProcessSessionInfoPacket(byte[] realData, Socket client)
    {
        if ((SessionInfo)realData[0] == SessionInfo.SessionSyncOK)
        {
            CheckAllPlayerSyncOK(realData, client);
        }
        else if ((SessionInfo)realData[0] == SessionInfo.PlayerNum)
        {
            SendPlayerNumber(realData, client);
        }
        else if ((SessionInfo)realData[0] == SessionInfo.LoadingOK)
        {
            CheckAllPlayerLoadingOK(realData, client);
        }
    }
    private void CheckAllPlayerLoadingOK(byte[] realData, Socket socket)
    {
        int playerNumber = BitConverter.ToInt32(realData.Skip(1).ToArray());

        // InGamePlayerInfo에서 해당 플레이어 찾기
        var inGamePlayerInfo = InGamePlayerInfos.FirstOrDefault(p => p.PlayerNumber == playerNumber);
        if (inGamePlayerInfo != null)
        {
            inGamePlayerInfo.IsLoadingOK = true;
        }
        // 여기에 모든 플레이어가 모두 연결되었는지 체크하는 로직
        bool allPlayersLoading = true;
        for (int i = 0; i < InGamePlayerInfos.Count; i++)
        {
            if (!InGamePlayerInfos[i].IsLoadingOK)
            {
                allPlayersLoading = false;
                break;
            }
        }

        if (allPlayersLoading)
        {
            // 모든 플레이어가 연결된 경우 수행할 동작
            OnAllPlayersLoadingOk();
        }
    }
    int PlayerNumber = 0;
    private void CheckAllPlayerSyncOK(byte[] userUid, Socket client)
    {
        long Useruid = BitConverter.ToInt64(userUid.Skip(1).ToArray());

        PlayerInfo player = new PlayerInfo();

        player.UserUID = Useruid;
        player.Socket = client;

        InGameSession.AddPlayer(player);

        // InGamePlayerInfo에서 해당 플레이어 찾기
        var inGamePlayerInfo = InGamePlayerInfos.FirstOrDefault(p => p.UserUID == Useruid);
        if (inGamePlayerInfo != null)
        {
            inGamePlayerInfo.IsConnected = true;
        }

        // 여기에 모든 플레이어가 모두 연결되었는지 체크하는 로직
        bool allPlayersConnected = true;
        for (int i = 0; i < InGamePlayerInfos.Count; i++)
        {
            if (!InGamePlayerInfos[i].IsConnected)
            {
                allPlayersConnected = false;
                break;
            }
        }

        if (allPlayersConnected)
        {
            // 모든 플레이어가 연결된 경우 수행할 동작
            OnAllPlayersConnected();
        }
    }
    private void OnAllPlayersConnected()
    {
        if (InGameSession == null) return;
        Packet packet = new Packet();

        int length = 0x01;

        packet.push((byte)InGameProtocol.SessionInfo);
        packet.push(length);
        packet.push((byte)SessionInfo.SessionSyncOK);

        InGameSession.SendToAllClient(packet);
    }
    private void OnAllPlayersLoadingOk()
    {
        if (InGameSession == null) return;
        Packet packet = new Packet();

        int length = 0x01;

        packet.push((byte)InGameProtocol.SessionInfo);
        packet.push(length);
        packet.push((byte)SessionInfo.AllPlayerLoadingOK);

        IsAllPlayerReady = true;

        InGameSession.SendToAllClient(packet);
    }
    private void SendPlayerNumber(byte[] uid, Socket client)
    {
        if (InGameSession == null) return;
        long useruid = BitConverter.ToInt64(uid.Skip(1).ToArray());
        for (int i = 0; i < InGamePlayerInfos.Count; i++)
        {
            if (InGamePlayerInfos[i].UserUID == useruid)
            {
                Packet packet = new Packet();

                int length = 0x01 + Utils.GetLength(InGamePlayerInfos[i].PlayerNumber);

                packet.push(((byte)InGameProtocol.SessionInfo));
                packet.push(length);
                packet.push(((byte)SessionInfo.PlayerNum));
                packet.push(InGamePlayerInfos[i].PlayerNumber);

                InGameSession.SendToClient(client, packet);
            }
        }
    }
}
