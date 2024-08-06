using System.Net.WebSockets;
using System.Collections.Concurrent;
using System.Threading.Tasks;

public static class ClientManager
{
    private static ConcurrentDictionary<long, WebSocket> _clients = new ConcurrentDictionary<long, WebSocket>();

    // 클라이언트 추가
    public static void AddClient(long accountId, WebSocket webSocket)
    {
        _clients[accountId] = webSocket;
    }

    // 클라이언트 제거
    public static void RemoveClient(long accountId)
    {
        _clients.TryRemove(accountId, out _);
    }

    public static async Task RemoveClientAsync(long accountId)
    {
        // 비동기 작업 예시
        await Task.Run(() => _clients.TryRemove(accountId, out _));
    }
    // 특정 클라이언트에게 메시지 전송
    public static async Task SendToClient(long accountId, Packet message)
    {
        if (_clients.TryGetValue(accountId, out WebSocket webSocket))
        {
            if (webSocket.State == WebSocketState.Open)
            {
                await webSocket.SendAsync(new ArraySegment<byte>(message.Buffer, 0, message.Position), WebSocketMessageType.Binary, true, CancellationToken.None);
            }
        }
    }

    // 모든 클라이언트에게 메시지 브로드캐스트
    public static async Task Broadcast(Packet message)
    {
        foreach (var client in _clients.Values)
        {
            if (client.State == WebSocketState.Open)
            {
                await client.SendAsync(new ArraySegment<byte>(message.Buffer, 0, message.Position), WebSocketMessageType.Binary, true, CancellationToken.None);
            }
        }
    }
    public static bool IsClientConnected(long accountId)
    {
        return _clients.TryGetValue(accountId, out WebSocket webSocket) && webSocket.State == WebSocketState.Open;
    }
    // WebSocket을 받아서 현재 접속한 유저인지 체크 후 메시지 전송
    public static async Task<bool> SendToSocket(WebSocket webSocket, Packet message)
    {
        // WebSocket이 _clients에 포함되어 있는지 확인
        if (webSocket == null)
        {
            Logger.SetLogger(LOGTYPE.ERROR, "webSocket is null");
            return false;
        }
        bool isClientContained = _clients.Values.Contains(webSocket);

        if (isClientContained && webSocket != null && webSocket.State == WebSocketState.Open)
        {
            await webSocket.SendAsync(new ArraySegment<byte>(message.Buffer, 0, message.Position), WebSocketMessageType.Binary, true, CancellationToken.None);
            return true;
        }
        return false;
    }
}
