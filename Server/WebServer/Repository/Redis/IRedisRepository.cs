namespace WebServer.Repository.Interface
{
    public interface IRedisRepository
    {
        Task<bool> SetSessionAsync(long accountId, string sessionId, TimeSpan expiration);
        Task<string> GetSessionAsync(long accountId);
        Task<bool> DeleteSessionAsync(long accountId);
    }
}