using SharedCode.Model;
using WebServer.Repository.Interface;

namespace WebServer.Service
{
    public class RankService
    {
        private readonly ILogger<RankService> _logger;
        private readonly IAccountRepository _accountRepository; // 추가

        public RankService(ILogger<RankService> logger, IAccountRepository accountRepository)
        {
            _logger = logger;
            _accountRepository = accountRepository; // 초기화
        }
        public async Task<(bool, List<RankRating>)> GetRank()
        {
            var result = await _accountRepository.GetTop10RanksAsync();
            return result;
        }
        public async Task<(bool, int)> GetMyRankRating(long accountId)
        {
            var dbResult = await _accountRepository.GetMyRating(accountId);
            var result = (dbResult.Item1, dbResult.Item3);
            return result;
        }
        public async Task<(bool, string)> SetMyRankRating(long accountId)
        {
            var myRating = await _accountRepository.GetMyRating(accountId);

            //여기에 랭크 점수처리 로직필요
            var rating = myRating.Item3 + 15;

            var result = await _accountRepository.SetMyRating(accountId, myRating.Item2, rating);
            if (result)
            {
                return (result, "SetRankRating Success");
            }
            else
            {
                return (result, "SetRankRating Fail");
            }
        }
        public async Task<(bool, int)> GetMyNormalRating(long accountId)
        {
            var dbResult = await _accountRepository.GetMyRating(accountId);
            var result = (dbResult.Item1, dbResult.Item2);
            return result;
        }
        public async Task<(bool, string)> SetMyNormalRating(long accountId)
        {
            var myRating = await _accountRepository.GetMyRating(accountId);

            //여기에 노말 점수처리 로직필요
            var rating = myRating.Item2 + 15;

            var result = await _accountRepository.SetMyRating(accountId, rating, myRating.Item3);
            if (result)
            {
                return (result, "SetNornamlRating Success");
            }
            else
            {
                return (result, "SetNornamlRating Fail");
            }
        }
    }
}
