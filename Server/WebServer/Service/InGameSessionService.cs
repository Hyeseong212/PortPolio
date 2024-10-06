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
        private readonly IAccountRepository _accountRepository; // �߰�
        private readonly WebSocketMatchService _webSocketMatchService; // �߰�

        public InGameSessionService(ILogger<InGameSessionService> logger, IAccountRepository accountRepository, WebSocketMatchService webSocketMatchService)
        {
            _logger = logger;
            _accountRepository = accountRepository; // �ʱ�ȭ
            _webSocketMatchService = webSocketMatchService;
        }
        public async Task<(bool, string)> NewSessionAsync(long sessionId, string ipAndPort)
        {
            //���� inGameSessionId�� WebSocketMatchService���� �ѱ�� createdSessionInfos
            string[] parts = ipAndPort.Split(':');
            string ipString = parts[0];
            int port = int.Parse(parts[1]);

            // IPAddress ��ü ����
            IPAddress ipAddress = IPAddress.Parse(ipString);

            // IPEndPoint ��ü ����
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
