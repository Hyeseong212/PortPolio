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

    private void CheckAllPlayerSyncOK(byte[] userUid, Socket client)
    {
        long Useruid = BitConverter.ToInt64(userUid.Skip(1).ToArray());

        // InGamePlayerInfo에서 해당 플레이어 찾기
        var inGamePlayerInfo = InGamePlayerInfos.FirstOrDefault(p => p.UserUID == Useruid);
        if (inGamePlayerInfo != null)
        {
            inGamePlayerInfo.IsConnected = true;
        }

        // PlayerInfo에서 해당 플레이어의 소켓 업데이트
        if (InGameSession == null) return;
        if (InGameSession.Users == null) return;
        var playerInfo = InGameSession.Users.FirstOrDefault(p => p.UserUID == Useruid);
        if (playerInfo != null)
        {
            playerInfo.Socket = client;
            InGameSession.SetSocket(playerInfo);
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
    private bool disposed = false;

    // 기타 리소스들...

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        if (disposing)
        {
            // 관리되는 리소스 해제
            // 예: 데이터 리스트 클리어, 이벤트 핸들러 제거 등
            InGamePlayerInfos.Clear();
            InGameSession = null;
        }

        // 관리되지 않는 리소스 해제
        // 필요한 경우 추가

        disposed = true;
    }

    ~SessionInfoManager()
    {
        Dispose(false);
    }
}
