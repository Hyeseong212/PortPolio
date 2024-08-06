using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using SharedCode.Model.HttpCommand;
using WebServer.Service;
using Microsoft.Extensions.Logging;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class SessionController : ControllerBase
    {
        private readonly ILogger<SessionController> _logger;
        private readonly SessionService _sessionService;

        public SessionController(ILogger<SessionController> logger, SessionService sessionService)
        {
            _logger = logger;
            _sessionService = sessionService;
        }
        [HttpPost]
        public async Task<RenewSessionResponse> RenewSession([FromBody] RenewSessionRequest request)
        {
            var (success, message) = await _sessionService.RenewSessionAsync(request.AccounId);
            return new RenewSessionResponse
            {
                IsSuccess = success,
                Message = message
            };
        }
    }
}
