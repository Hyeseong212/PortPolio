using Newtonsoft.Json;
using SharedCode.Model;
using SharedCode.Model.HttpCommand;
using System.Net;
using WebServer.Repository;
using WebServer.Repository.Interface;

namespace WebServer.Service
{
    public class InGameSessionService
    {
        private readonly ILogger<InGameSessionService> _logger;
        private readonly IAccountRepository _accountRepository; // 추가
        private readonly WebSocketMatchService _webSocketMatchService; // 추가

        public InGameSessionService(ILogger<InGameSessionService> logger, IAccountRepository accountRepository, WebSocketMatchService webSocketMatchService)
        {
            _logger = logger;
            _accountRepository = accountRepository; // 초기화
            _webSocketMatchService = webSocketMatchService;
        }
        public async Task<(bool, string)> NewSessionAsync(long sessionId, string ipAndPort)
        {
            //받은 inGameSessionId로 WebSocketMatchService에게 넘기고 createdSessionInfos
            string[] parts = ipAndPort.Split(':');
            string ipString = parts[0];
            int port = int.Parse(parts[1]);

            // IPAddress 객체 생성
            IPAddress ipAddress = IPAddress.Parse(ipString);

            // IPEndPoint 객체 생성
            IPEndPoint endPoint = new IPEndPoint(ipAddress, port);
            var isSuccess = await _webSocketMatchService.SendSessionIPAndPortToClient(sessionId, endPoint);
            if (isSuccess)
            {
                return (true, "Send Success");
            }
            else
            {
                return (false, "Send Fail");
            }
        }
    }
}
