using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using WebServer.Repository.Interface;

namespace WebServer.Repository
{
    public class RedisRepository : IRedisRepository
    {
        private readonly ILogger<RedisRepository> _logger;
        private readonly IDatabase _database;

        public RedisRepository(ILogger<RedisRepository> logger, RedisContext redisContext)
        {
            _logger = logger;
            _database = redisContext.RedisDB;
        }

        public async Task<bool> SetSessionAsync(long accountId, string sessionId, TimeSpan expiration)
        {
            return await _database.StringSetAsync(accountId.ToString(), sessionId, expiration);
        }

        public async Task<string> GetSessionAsync(long accountId)
        {
            return await _database.StringGetAsync(accountId.ToString());
        }

        public async Task<bool> DeleteSessionAsync(long accountId)
        {
            return await _database.KeyDeleteAsync(accountId.ToString());
        }
    }
}
