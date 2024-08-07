using SharedCode.Model;
using System.Net.Sockets;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using MessagePack;
using SharedCode.MessagePack;

internal class SessionInfoManager : IDisposable
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
        var packet = MessagePackSerializer.Deserialize<Packet>(realData);
        if ((SessionInfo)packet.Data[0] == SessionInfo.SessionSyncOK)
        {
            CheckAllPlayerSyncOK(packet.Data, client);
        }
        else if ((SessionInfo)packet.Data[0] == SessionInfo.PlayerNum)
        {
            SendPlayerNumber(packet.Data, client);
        }
        else if ((SessionInfo)packet.Data[0] == SessionInfo.LoadingOK)
        {
            CheckAllPlayerLoadingOK(packet.Data, client);
        }
    }

    private void CheckAllPlayerLoadingOK(byte[] realData, Socket socket)
    {
        int playerNumber = BitConverter.ToInt32(realData.Skip(1).ToArray());

        var inGamePlayerInfo = InGamePlayerInfos.FirstOrDefault(p => p.PlayerNumber == playerNumber);
        if (inGamePlayerInfo != null)
        {
            inGamePlayerInfo.IsLoadingOK = true;
        }

        bool allPlayersLoading = InGamePlayerInfos.All(p => p.IsLoadingOK);

        if (allPlayersLoading)
        {
            OnAllPlayersLoadingOk();
        }
    }

    private void CheckAllPlayerSyncOK(byte[] userUid, Socket client)
    {
        long userUidValue = BitConverter.ToInt64(userUid.Skip(1).ToArray());

        var inGamePlayerInfo = InGamePlayerInfos.FirstOrDefault(p => p.UserUID == userUidValue);
        if (inGamePlayerInfo != null)
        {
            inGamePlayerInfo.IsConnected = true;
        }

        if (InGameSession != null && InGameSession.Users != null)
        {
            var playerInfo = InGameSession.Users.FirstOrDefault(p => p.UserUID == userUidValue);
            if (playerInfo != null)
            {
                playerInfo.Socket = client;
                InGameSession.SetSocket(playerInfo);
            }
        }

        bool allPlayersConnected = InGamePlayerInfos.All(p => p.IsConnected);

        if (allPlayersConnected)
        {
            OnAllPlayersConnected();
        }
    }

    private void OnAllPlayersConnected()
    {
        if (InGameSession == null) return;

        var packet = new Packet
        {
            Protocol = (byte)InGameProtocol.SessionInfo,
            Data = new byte[] { (byte)SessionInfo.SessionSyncOK }
        };

        var serializedData = MessagePackSerializer.Serialize(packet);
        InGameSession.SendToAllClient(serializedData);
    }

    private void OnAllPlayersLoadingOk()
    {
        if (InGameSession == null) return;

        var packet = new Packet
        {
            Protocol = (byte)InGameProtocol.SessionInfo,
            Data = new byte[] { (byte)SessionInfo.AllPlayerLoadingOK }
        };

        var serializedData = MessagePackSerializer.Serialize(packet);
        IsAllPlayerReady = true;

        InGameSession.SendToAllClient(serializedData);
    }

    private void SendPlayerNumber(byte[] uid, Socket client)
    {
        if (InGameSession == null) return;
        long userUid = BitConverter.ToInt64(uid.Skip(1).ToArray());
        var inGamePlayerInfo = InGamePlayerInfos.FirstOrDefault(p => p.UserUID == userUid);
        if (inGamePlayerInfo != null)
        {
            var playerNumberPacket = new PlayerNumberPacket
            {
                PlayerNumber = inGamePlayerInfo.PlayerNumber
            };

            var data = MessagePackSerializer.Serialize(playerNumberPacket);
            var packet = new Packet
            {
                Protocol = (byte)InGameProtocol.SessionInfo,
                Data = data
            };

            var serializedData = MessagePackSerializer.Serialize(packet);
            InGameSession.SendToClient(client, serializedData);
        }
    }

    private bool disposed = false;

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
            InGamePlayerInfos.Clear();
            InGameSession = null;
        }

        disposed = true;
    }

    ~SessionInfoManager()
    {
        Dispose(false);
    }
}
