using Newtonsoft.Json;
using SharedCode.Model;
using SharedCode.Model.HttpCommand;
using WebServer.Repository;
using WebServer.Repository.Interface;

namespace WebServer.Service
{
    public class GuildService
    {
        private readonly ILogger<GuildService> _logger;
        private readonly IAccountRepository _accountRepository; // 추가

        public GuildService(ILogger<GuildService> logger, IAccountRepository accountRepository)
        {
            _logger = logger;
            _accountRepository = accountRepository; // 초기화
        }
        public async Task<(bool, string)> GuildCreateAsync(long creatorId,string guildName)
        {
            var isSuccess = await _accountRepository.CreateGuildAsync(creatorId, guildName);
            if (isSuccess)
            {
                return (true, "Guild Create Success");
            }
            else
            {
                return (false, "Guild Create Fail");
            }
        }
        public async Task<(bool, string ,string)> FindGuildAsync(string guildName)
        {
            var isSuccess = await _accountRepository.FindGuildAsync(guildName);
            if (isSuccess.Item1)
            {
                string guildInfo = JsonConvert.SerializeObject(isSuccess.Item2);

                return (true, "Find Guild Success", guildInfo);
            }
            else
            {
                return (true, "Find Guild Fail", "");
            }
        }
        public async Task<(bool, string)> RequestingJoinGuildAsync(long guildId, long accountId)
        {
            var isSuccess = await _accountRepository.SetGuildRequestUserAsync(guildId, accountId);
            if (isSuccess)
            {
                return (true, "Join Request Success");
            }
            else
            {
                return (true, "Join Request Fail");
            }
        }
        public async Task<(bool, string, string)> GetGuildInfoAsync(long guildId)
        {
            var isSuccess = await _accountRepository.GetGuildInfoAsync(guildId);
            if (isSuccess.Item1)
            {
                if (isSuccess.Item2 == null) return (false, "GetGuildInfo Request Fail", "");
                return (true, "GetGuildInfo Request Success", isSuccess.Item2);
            }
            else
            {
                return (true, "GetGuildInfo Request Fail", "");
            }
        }
        public async Task<(bool, string, string)> GetGuildJoinRequestAsync(long guildId)
        {
            var isSuccess = await _accountRepository.GetGuildJoinRequestAsync(guildId);
            if (isSuccess.Item1)
            {
                if (isSuccess.Item2 == null) return (false, "GetGuildInfo Request Fail", "");
                return (true, "GetGuildInfo Request Success", isSuccess.Item2);
            }
            else
            {
                return (true, "GetGuildInfo Request Fail", "");
            }
        }
        public async Task<(bool, string)> GuildJoinOKAsync(long accountId, long guildId)
        {
            var isSuccess = await _accountRepository.GuildJoinOKAsync(accountId, guildId);
            if (isSuccess)
            {
                return (true, "Join Request Success");
            }
            else
            {
                return (true, "Join Request Fail");
            }
        }
        public async Task<(bool, string)> GuildResignAsync(long accountId)
        {
            var isSuccess = await _accountRepository.GuildResignAsync(accountId);
            if (isSuccess)
            {
                return (true, "Guild Resign Success");
            }
            else
            {
                return (true, "Guild Resign Fail");
            }
        }
    }
}
