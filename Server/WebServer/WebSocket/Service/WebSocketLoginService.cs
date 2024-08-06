using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Text;
using WebServer.Model;
using WebServer.Repository.Interface;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class WebSocketLoginService
{
    private readonly IAccountRepository _accountRepository;

    // 생성자에서 IAccountRepository와 ClientManager 주입
    public WebSocketLoginService(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task ProcessLoginPacket(WebSocket webSocket, byte[] realData)
    {
        // 데이터 구분
        if(realData.Length < 1)
        {
            return;
        }
        LoginRequestType loginRequestType = (LoginRequestType)realData[0];
        if (loginRequestType == LoginRequestType.SignupRequest)
        {
            // 구현안함.
        }
        else if (loginRequestType == LoginRequestType.LoginRequest)
        {
            byte[] UserUid = realData.Skip(1).ToArray();
            await Login(UserUid, webSocket);
        }
        else if (loginRequestType == LoginRequestType.LogoutRequest)
        {
            // 로그아웃 작업
            await Logout(realData, webSocket);
        }
        else if (loginRequestType == LoginRequestType.DeleteRequest)
        {
            // 회원 삭제 작업
        }
        else if (loginRequestType == LoginRequestType.UpdateRequest)
        {
            // 회원정보 수정 작업
        }
    }

    private async Task Logout(byte[] uid, WebSocket webSocket)
    {
        // dataPacket에서 유저 ID와 패스워드 추출
        var accountId = BitConverter.ToInt64(uid, 0);
        // 유저 정보 확인
        // 로그인 성공 시 클라이언트를 ClientManager에 추가
        await ClientManager.RemoveClientAsync(accountId);
    }

    private async Task Login(byte[] dataPacket, WebSocket webSocket)
    {
        var accountId = BitConverter.ToInt64(dataPacket, 0);

        var account = await _accountRepository.GetAccountWithAccountIdAsync(accountId);
        if (account != null)
        {
            // 로그인 성공 시 클라이언트를 ClientManager에 추가
            ClientManager.AddClient(accountId, webSocket);
        }
        else
        {
        }
    }
}
