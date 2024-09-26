using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebServer.Controllers;
using WebServer.Repository.Interface;
using WebServer.Service;

namespace UnitTestProject
{
    public class InGameSessionControllerTest
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ILogger<InGameSessionService> _logger;
        private readonly InGameSessionService _inGameSessionService;
        private readonly WebSocketMatchService _webSocketMatchService; // 추가
        private readonly InGameSessionController _inGameSessionController;
        public InGameSessionControllerTest()
        {
            _accountRepository = Substitute.For<IAccountRepository>();
            _logger = Substitute.For<ILogger<InGameSessionService>>();
            _webSocketMatchService = Substitute.For<WebSocketMatchService>(); // WebSocketMatchService 모킹

            // 실제 AccountService 인스턴스 생성
            _inGameSessionService = new InGameSessionService(_logger, _accountRepository, _webSocketMatchService);

            // AccountController 생성
            _inGameSessionController = new InGameSessionController(Substitute.For<ILogger<InGameSessionController>>(), _inGameSessionService);
        }
    }
}
