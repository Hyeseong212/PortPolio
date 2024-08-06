using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using WebServer.Repository.Interface;

namespace WebServer.Service
{
    public class SessionService
    {
        private readonly ILogger<SessionService> _logger;
        private readonly IAccountRepository _accountRepository; // �߰�
        private readonly IRedisRepository _redisRepository;

        public SessionService(ILogger<SessionService> logger, IAccountRepository accountRepository, IRedisRepository redisRepository)
        {
            _logger = logger;
            _accountRepository = accountRepository; // �ʱ�ȭ
            _redisRepository = redisRepository;
        }

        public async Task<(bool, string)> RenewSessionAsync(long accountId)
        {
            var sessionData = await _redisRepository.GetSessionAsync(accountId);
            if (sessionData == null)
            {
                string message = "Session not found";
                _logger.LogWarning(message);
                return (false, message);
            }

            var sessionExpiration = TimeSpan.FromMinutes(5); // ���� ��ȿ �Ⱓ ���� (��: 1�ð�)

            var sessionRenewed = await _redisRepository.SetSessionAsync(accountId, sessionData, sessionExpiration);
            if (!sessionRenewed)
            {
                string message = "Failed to renew session";
                _logger.LogError(message);
                return (false, message);
            }

            return (true, "Session renewed successfully");
        }
        public async Task<bool> ValidateSessionAsync(long accountId, string sessionId)
        {
            var storedSessionId = await _redisRepository.GetSessionAsync(accountId);
            return storedSessionId == sessionId;
        }
    }
}
