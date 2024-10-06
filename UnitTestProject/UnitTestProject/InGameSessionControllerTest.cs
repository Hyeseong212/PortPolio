using Microsoft.Extensions.Logging;
using NSubstitute;
using SharedCode.Model;
using System.Net.WebSockets;
using System.Net;
using WebServer.Repository.Interface;
using WebServer.Service;
namespace UnitTestProject
{
    public class InGameSessionServiceTest
    {
        private readonly ILogger<InGameSessionService> _logger;
        private readonly IAccountRepository _accountRepository;
        private readonly WebSocketMatchService _webSocketMatchService; // 인터페이스 대신 실제 클래스 사용
        private readonly InGameSessionService _inGameSessionService;

        public InGameSessionServiceTest()
        {
            _logger = Substitute.For<ILogger<InGameSessionService>>();
            _accountRepository = Substitute.For<IAccountRepository>();

            // 실제 WebSocketMatchService를 사용
            _webSocketMatchService = new WebSocketMatchService(_accountRepository);

            // 서비스 인스턴스 생성
            _inGameSessionService = new InGameSessionService(_logger, _accountRepository, _webSocketMatchService);
        }

        [Fact]
        public async Task TestNewSessionAsync_Success()
        {
            // Arrange
            var sessionId = 1L;
            var ipAndPort = "127.0.0.1:8080";
            var ipAddress = IPAddress.Parse("127.0.0.1");
            var endPoint = new IPEndPoint(ipAddress, 8080);

            // 필요한 데이터를 추가하여 waitQueue를 채워줍니다.
            var playerInfo = new PlayerInfo
            {
                UserUID = 12345,
                WebSocket = Substitute.For<WebSocket>()
            };

            var playerList = new List<PlayerInfo> { playerInfo };

            // WebSocketMatchService 인스턴스의 waitQueue에 플레이어를 추가
            var matchService = new WebSocketMatchService(Substitute.For<IAccountRepository>());
            var waitQueueField = typeof(WebSocketMatchService).GetField("waitQueue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var waitQueue = (Queue<List<PlayerInfo>>)waitQueueField.GetValue(matchService);
            waitQueue.Enqueue(playerList);  // waitQueue에 데이터를 추가

            // 실제 WebSocketMatchService를 사용한 테스트
            var inGameSessionService = new InGameSessionService(Substitute.For<ILogger<InGameSessionService>>(), Substitute.For<IAccountRepository>(), matchService);

            // Act
            var (success, message) = await inGameSessionService.NewSessionAsync(sessionId, ipAndPort);

            // Assert
            Assert.True(success);
            Assert.Equal("Send Success", message);
        }

    }
}
