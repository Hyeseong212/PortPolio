using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using SharedCode.Model.HttpCommand;
using WebServer.Service;
using Microsoft.Extensions.Logging;
using SharedCodeLibrary.HttpCommand;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class InGameSessionController : ControllerBase
    {
        private readonly ILogger<InGameSessionController> _logger;
        private readonly InGameSessionService _inGameSessionService;

        public InGameSessionController(ILogger<InGameSessionController> logger, InGameSessionService inGamesessionService)
        {
            _logger = logger;
            _inGameSessionService = inGamesessionService;
        }
        [HttpPost]
        public async Task<RenewSessionResponse> CreatedSessionIPEndPointSendToClient([FromBody] NewInGameSessionRequest request)
        {
            var (success, message) = await _inGameSessionService.NewSessionAsync(long.Parse(request.SessionId), request.IPandPort);
            return new RenewSessionResponse
            {
                IsSuccess = success,
                Message = message
            };
        }
    }
}
