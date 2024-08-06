using Microsoft.AspNetCore.Mvc;
using SharedCode.Model.HttpCommand;
using WebServer.Service;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class GuildController : ControllerBase
    {
        private readonly ILogger<GuildController> _logger;
        private readonly GuildService _guildService;

        public GuildController(ILogger<GuildController> logger, GuildService guildService)
        {
            _logger = logger;
            _guildService = guildService;
        }
        [HttpPost]
        public async Task<GuildCreateResponse> GuildCreate(GuildCreateRequest request)
        {
            var (isSuccess, message) = await _guildService.GuildCreateAsync(request.Creator,request.GuildName);
            return new GuildCreateResponse
            {
                IsSuccess = isSuccess,
                Message = message
            };
        }
        [HttpPost]
        public async Task<FindGuildResponse> GuildFind(FindGuildRequest request)
        {
            var (isSuccess, message, guildInfo) = await _guildService.FindGuildAsync(request.GuildName);
            return new FindGuildResponse
            {
                IsSuccess = isSuccess,
                Message = message,
                GuildInfo = guildInfo
            };
        }
        [HttpPost]
        public async Task<RequestingJoinGuildResponse> RequestingJoinGuild(RequestingJoinGuildRequest request)
        {
            var (isSuccess, message) = await _guildService.RequestingJoinGuildAsync(request.GuildId , request.AccountId);
            return new RequestingJoinGuildResponse
            {
                IsSuccess = isSuccess,
                Message = message
            };
        }
        [HttpPost]
        public async Task<GetGuildInfoResponse> GetGuildInfo(GetGuildInfoRequest request)
        {
            var (isSuccess, message, myGuildInfo) = await _guildService.GetGuildInfoAsync(request.GuildId);
            return new GetGuildInfoResponse
            {
                IsSuccess = isSuccess,
                Message = message,
                MyGuildInfo = myGuildInfo
            };
        }
        [HttpPost]
        public async Task<GetGuildJoinResponse> GetGuildJoinRequestInfo(GetGuildJoinRequest request)
        {
            var (isSuccess, message, guildRequestInfo) = await _guildService.GetGuildJoinRequestAsync(request.GuildId);
            return new GetGuildJoinResponse
            {
                IsSuccess = isSuccess,
                Message = message,
                GuildRequestInfo = guildRequestInfo
            };
        }
        [HttpPost]
        public async Task<GuildJoinOkResponse> GuildJoinOK(GuildJoinOkRequest request)
        {
            var (isSuccess, message) = await _guildService.GuildJoinOKAsync(request.AccountId, request.GuildId);
            return new GuildJoinOkResponse
            {
                IsSuccess = isSuccess,
                Message = message,
            };
        }
        [HttpPost]
        public async Task<GuildJoinOkResponse> GuildResign(GuildResignRequest request)
        {
            var (isSuccess, message) = await _guildService.GuildResignAsync(request.AccountId);
            return new GuildJoinOkResponse
            {
                IsSuccess = isSuccess,
                Message = message,
            };
        }
    }
}
