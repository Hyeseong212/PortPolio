using MessagePack;
using System.Net.WebSockets;
using System.Text;

public static class WebSocketHandler
{
    private static WebSocketChatService _chatService;
    private static WebSocketMatchService _matchService;
    private static WebSocketLoginService _loginService;

    public static void Configure(WebSocketChatService chatService, WebSocketMatchService matchService, WebSocketLoginService loginService)
    {
        _chatService = chatService;
        _matchService = matchService;
        _loginService = loginService;
    }

    public static void MapWebSocketHandlers(this IApplicationBuilder app)
    {
        app.Map("/ws", HandleWebSocketRequests);
    }

    private static void HandleWebSocketRequests(IApplicationBuilder app)
    {
        app.Use(WebSocketMiddleware);
    }

    private static async Task WebSocketMiddleware(HttpContext context, Func<Task> next)
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            await ProcessWebSocketRequests(context, webSocket);
        }
        else
        {
            context.Response.StatusCode = 400;
        }
    }

    private static async Task ProcessWebSocketRequests(HttpContext context, WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];
        WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        while (!result.CloseStatus.HasValue)
        {
            // 패킷을 분석하여 처리하는 메서드 호출
            await HandlePacket(webSocket, buffer, result.Count);

            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }
        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
    }

    private static async Task HandlePacket(WebSocket webSocket, byte[] buffer, int count)
    {
        try
        {
            var packet = MessagePackSerializer.Deserialize<SharedCode.MessagePack.Packet>(buffer.Take(count).ToArray());

            switch (packet.Protocol)
            {
                case (byte)Protocol.Chat:
                    if (_chatService == null) return;
                    await _chatService.ProcessChatPacket(webSocket, packet.Data);
                    break;
                case (byte)Protocol.Match:
                    if (_matchService == null) return;
                    await _matchService.ProcessMatchPacket(webSocket, packet.Data);
                    break;
                case (byte)Protocol.Login:
                    if (_loginService == null) return;
                    await _loginService.ProcessLoginPacket(webSocket, packet.Data);
                    break;
                default:
                    break;
            }
        }
        catch (Exception ex)
        {
            Logger.SetLogger(LOGTYPE.ERROR, ex.Message);
        }
    }
}
