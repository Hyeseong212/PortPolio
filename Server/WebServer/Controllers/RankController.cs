using Microsoft.AspNetCore.Mvc;
using SharedCode.Model.HttpCommand;
using WebServer.Service;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class RankController : ControllerBase
    {
        private readonly ILogger<RankController> _logger;
        private readonly RankService _rankService;

        public RankController(ILogger<RankController> logger, RankService rankService)
        {
            _logger = logger;
            _rankService = rankService;
        }
        [HttpGet]
        public async Task<GetRankResponse> GetRank()
        {
            var (isSuccess, message) = await _rankService.GetRank();

            return new GetRankResponse
            {
                IsSuccess = isSuccess,
                RankList = message
            };
        }
        [HttpGet]
        public async Task<GetMyRankResponse> GetMyRankRating(GetMyRankRequest getMyRankRequest)
        {
            var (isSuccess, rating) = await _rankService.GetMyRankRating(getMyRankRequest.AccountId);
            return new GetMyRankResponse
            {
                IsSuccess = isSuccess,
                Rating = rating
            };
        }
        [HttpPost]
        public async Task<SetMyRankResponse> SetMyRankRating(SetMyRankRequest setMyRankRequest)
        {
            var (isSuccess, message) = await _rankService.SetMyRankRating(setMyRankRequest.AccountId);
            return new SetMyRankResponse
            {
                IsSuccess = isSuccess,
                Message = message
            };
        }
        [HttpGet]
        public async Task<GetMyRankResponse> GetMyNormalRating(GetMyRankRequest getMyRankRequest)
        {
            var (isSuccess, rating) = await _rankService.GetMyRankRating(getMyRankRequest.AccountId);
            return new GetMyRankResponse
            {
                IsSuccess = isSuccess,
                Rating = rating
            };
        }
        [HttpPost]
        public async Task<SetMyRankResponse> SetMyNormalRating(SetMyRankRequest setMyRankRequest)
        {
            var (isSuccess, message) = await _rankService.SetMyRankRating(setMyRankRequest.AccountId);
            return new SetMyRankResponse
            {
                IsSuccess = isSuccess,
                Message = message
            };
        }
    }
}
