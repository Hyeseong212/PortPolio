using MessagePack;
using SharedCode.MessagePack;
using System.Net.WebSockets;
using WebServer.Model;
using WebServer.Repository.Interface;

public class WebSocketLoginService
{
    private readonly IAccountRepository _accountRepository;

    // 생성자에서 IAccountRepository 주입
    public WebSocketLoginService(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task ProcessLoginPacket(WebSocket webSocket, byte[] realData)
    {
        var loginRequest = MessagePackSerializer.Deserialize<LoginRequestPacket>(realData);

        switch (loginRequest.RequestType)
        {
            case LoginRequestType.SignupRequest:
                // 구현안함.
                break;
            case LoginRequestType.LoginRequest:
                await Login(loginRequest.UserId, webSocket);
                break;
            case LoginRequestType.LogoutRequest:
                await Logout(loginRequest.UserId, webSocket);
                break;
            case LoginRequestType.DeleteRequest:
                // 회원 삭제 작업
                break;
            case LoginRequestType.UpdateRequest:
                // 회원정보 수정 작업
                break;
            default:
                break;
        }
    }

    private async Task Logout(long accountId, WebSocket webSocket)
    {
        await ClientManager.RemoveClientAsync(accountId);
    }

    private async Task Login(long accountId, WebSocket webSocket)
    {
        var account = await _accountRepository.GetAccountWithAccountIdAsync(accountId);
        var responsePacket = new LoginResponsePacket();

        if (account != null)
        {
            ClientManager.AddClient(accountId, webSocket);
            responsePacket.Success = true;
            responsePacket.Message = "Login successful";
        }
        else
        {
            responsePacket.Success = false;
            responsePacket.Message = "Login failed";
        }

        var responseBytes = MessagePackSerializer.Serialize(responsePacket);
        await ClientManager.SendToSocket(webSocket, responseBytes);
    }
}
