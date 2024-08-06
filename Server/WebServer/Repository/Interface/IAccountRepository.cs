using SharedCode.Model;
using WebServer.Model;
using WebServer.Model.DbEntity;

namespace WebServer.Repository.Interface
{
    public interface IAccountRepository
    {
        Task<bool> IsAlreadyExistAsync(string id);
        Task<bool> CreateAccountAsync(string id, string pw, string nickName);
        Task<AccountEntity> GetAccountAsync(string id);
        Task<AccountEntity> GetAccountWithAccountIdAsync(long accountId);
        Task<bool> ModifyNickName(long accountId, string nickname);
        Task<long> GetGoldAsync(long accountId);
        Task<bool> UpdateGoldAsync(long accountId, long newGoldAmount);
        Task<bool> CheckGoldAsync(long accountId, long itemGold);
        Task<bool> BuyCharacterAsync(long accountId, CharacterStore item);
        Task<bool> BuyItemAsync(long accountId, Item item, int count);
        Task<bool> SellItemAsync(long accountId, Item item, int count);
        Task<(bool, List<RankRating>)> GetTop10RanksAsync();
        Task<(bool, int, int)> GetMyRating(long accountId);
        Task<bool> SetMyRating(long accountId, int normalRating, int rankRating);
        Task<AccountNickNameEntity> GetNickName(long accountId);
        Task<(bool, long)> GetAccountGuildId(long accountId);
        Task<bool> SetAccountGuildId(long accountId, long guildId);
        Task<bool> CreateGuildAsync(long creatorId, string guildName);
        Task<(bool, List<GuildInfo>)> FindGuildAsync(string guildName);
        Task<GuildEntity> GetGuildIdAsync(long guildId);
        Task<bool> SetGuildRequestUserAsync(long guildId, long accountId);
        Task<(bool, string)> GetGuildInfoAsync(long guildId);
        Task<(bool, string)> GetGuildJoinRequestAsync(long guildId);
        Task<bool> GuildJoinOKAsync(long accountId, long guildId);
        Task<bool> GuildResignAsync(long accountId);
        Task<(bool, List<long>)> GetAllGuildUsers(long guildId);
    }
}