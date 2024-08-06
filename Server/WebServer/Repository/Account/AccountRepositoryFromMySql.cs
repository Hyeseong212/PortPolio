using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SharedCode.Model;
using WebServer.Model;
using WebServer.Model.DbEntity;
using WebServer.Repository.Interface;

namespace WebServer.Repository
{
    public class AccountRepositoryFromMySql : IAccountRepository
    {
        private readonly ILogger<AccountRepositoryFromMySql> _logger;
        private readonly AccountDbContext _accountDbContext;

        public AccountRepositoryFromMySql(ILogger<AccountRepositoryFromMySql> logger, AccountDbContext accountDbContext)
        {
            _logger = logger;
            _accountDbContext = accountDbContext;
        }

        public async Task<bool> IsAlreadyExistAsync(string id)
        {
            return await _accountDbContext.Account.AnyAsync(x => x.UserId == id);
        }

        public async Task<bool> CreateAccountAsync(string id, string pw, string nickName)
        {
            // 중복된 유저가 있는지 확인
            if (await IsAlreadyExistAsync(id))
            {
                return false;
            }

            // 계정 생성 및 AccountId 가져오기
            var accountId = await CreateAccountInfo(id, pw);

            if (accountId != -1)
            {
                // 추가 정보 생성
                await CreateCurrencyInfo(accountId);
                await CreateCharacterInfo(accountId);
                await CreateNickName(accountId, nickName);
                await CreateFirstGuildDataAsync(accountId);
                await CreateAccountRating(accountId);

                return true;
            }

            return false;
        }
        
        public async Task<bool> ModifyNickName(long accountId, string nickname)
        {
            var accountNickNameEntity = await SelectNickNameTable(accountId);
            if (accountNickNameEntity != null)
            {
                accountNickNameEntity.AccountNickName = nickname;
                _accountDbContext.AccountNickName.Update(accountNickNameEntity);
                await _accountDbContext.SaveChangesAsync();
                return true;
            }
            return false; // 계정 ID가 없는 경우 false 반환
        }

        public async Task<AccountNickNameEntity> GetNickName(long accountId)
        {
            var nickname = await _accountDbContext.AccountNickName
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.AccountId == accountId);

            if (nickname == null)
            {
                // 적절한 예외를 던지거나 기본값을 반환
                throw new Exception("Nickname not found");
                // 또는 기본값 반환
                // return new AccountNickNameEntity { /* 기본값 초기화 */ };
            }

            return nickname;
        }
        public async Task<AccountEntity> GetAccountAsync(string id)
        {
            var account = await _accountDbContext.Account.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == id);
            if (account == null)
            {
                throw new Exception("Account not found");
            }
            return account;
        }

        public async Task<AccountEntity> GetAccountWithAccountIdAsync(long accountId)
        {
            var account = await _accountDbContext.Account.AsNoTracking().SingleOrDefaultAsync(x => x.AccountId == accountId);
            if (account == null)
            {
                throw new Exception("Account not found");
            }
            return account;
        }

        public async Task<long> GetGoldAsync(long accountId)
        {
            var accountCurrency = await _accountDbContext.AccountCurrency
                .AsNoTracking()
                .SingleOrDefaultAsync(ac => ac.AccountId == accountId);

            return accountCurrency.Gold;
        }

        public async Task<bool> UpdateGoldAsync(long accountId, long newGoldAmount)
        {
            var accountCurrency = await _accountDbContext.AccountCurrency
                .SingleOrDefaultAsync(ac => ac.AccountId == accountId);

            if (accountCurrency == null)
            {
                return false;
            }

            accountCurrency.Gold = newGoldAmount;
            await _accountDbContext.SaveChangesAsync();
            return true;
        }

        // 트랜잭션을 걸어서 테스트하는 방법
        // 여러개의 디비를 트랜잭션으로 묶을 수 는 없음
        public async Task<bool> BuyItemAsync(long accountId, Item item, int count)
        {
            using var transaction = await _accountDbContext.Database.BeginTransactionAsync();

            try
            {
                // 해당 계정의 인벤토리에서 item_id가 일치하는 항목을 찾기
                var inventoryItem = await _accountDbContext.Inventory
                    .SingleOrDefaultAsync(inv => inv.AccountId == accountId && inv.ItemId == item.ItemId);

                if (inventoryItem != null)
                {
                    // 이미 존재하는 아이템의 경우 수량 업데이트
                    inventoryItem.Count += count; // 구매 시 입력받은 수량만큼 증가
                }
                else
                {
                    // 존재하지 않는 아이템의 경우 새 항목 추가
                    _accountDbContext.Inventory.Add(new InventoryEntity()
                    {
                        AccountId = accountId,
                        ItemId = item.ItemId,
                        Count = count // 최초 구매 시 입력받은 수량으로 설정
                    });
                }

                await _accountDbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                Logger.SetLogger(LOGTYPE.ERROR, ex.Message);
                await transaction.RollbackAsync();
                // 에러 처리 로직 추가 가능
                throw; // 필요시 에러를 다시 던질 수 있음
            }

            return true;
        }
        public async Task<bool> SellItemAsync(long accountId, Item item, int count)
        {
            using var transaction = await _accountDbContext.Database.BeginTransactionAsync();

            if (item == null) return false; 
            try
            {
                // 해당 계정의 인벤토리에서 item_id가 일치하는 항목을 찾기
                var inventoryItem = await _accountDbContext.Inventory
                    .SingleOrDefaultAsync(inv => inv.AccountId == accountId && inv.ItemId == item.ItemId);

                if (inventoryItem != null)
                {
                    // 존재하는 아이템의 경우 수량 감소
                    if (inventoryItem.Count >= count)
                    {
                        inventoryItem.Count -= count; // 판매 시 입력받은 수량만큼 감소

                        if (inventoryItem.Count == 0)
                        {
                            // 수량이 0이 되면 인벤토리에서 항목 삭제
                            _accountDbContext.Inventory.Remove(inventoryItem);
                        }
                    }
                    else
                    {
                        // 수량이 부족한 경우 예외 처리
                        throw new InvalidOperationException("Cannot sell more items than are in the inventory.");
                    }
                }
                else
                {
                    // 인벤토리에 아이템이 없는 경우 예외 처리
                    throw new InvalidOperationException("Cannot sell an item that is not in the inventory.");
                }

                await _accountDbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                Logger.SetLogger(LOGTYPE.ERROR, ex.Message);
                await transaction.RollbackAsync();
                // 에러 처리 로직 추가 가능
                throw; // 필요시 에러를 다시 던질 수 있음
            }

            return true;
        }
        private async Task<bool> CreateAccountRating(long accountId)
        {
            var accountRating = new AccountRatingEntity
            {
                AccountId = accountId,
                NormalRating = 1000,
                RankRating = 1000 // RankRating도 초기화가 필요하다면 설정
            };

            // 데이터베이스 컨텍스트에 accountRating을 추가
            await _accountDbContext.AccountRating.AddAsync(accountRating);
            await _accountDbContext.SaveChangesAsync();

            return true;
        }
        private async Task<long> CreateAccountInfo(string id, string pw)
        {
            var accountEntity = new AccountEntity
            {
                UserId = id,
                UserPassword = pw
            };
            await _accountDbContext.Account.AddAsync(accountEntity);
            await _accountDbContext.SaveChangesAsync();

            return accountEntity.AccountId; // 생성된 AccountId 반환
        }
        private async Task<bool> CreateFirstGuildDataAsync(long accountId)
        {
            var accountGuildEntity = new AccountGuildEntity
            {
                AccountId = accountId,
                GuildId = -1,
                UserClass = (int)GUILDCLASS.NONE
            };
            await _accountDbContext.AccountGuild.AddAsync(accountGuildEntity);
            await _accountDbContext.SaveChangesAsync();

            return true;
        }

        private async Task<bool> CreateNickName(long accountId, string nickName)
        {
            if (nickName == null) return false;
            var accountNickNameEntity = new AccountNickNameEntity
            {
                AccountId = accountId,
                AccountNickName = nickName
            };
            await _accountDbContext.AccountNickName.AddAsync(accountNickNameEntity);
            await _accountDbContext.SaveChangesAsync();

            return true;
        }
        private async Task<AccountNickNameEntity> SelectNickNameTable(long accountId)
        {
            var nickname = await _accountDbContext.AccountNickName.AsNoTracking().SingleOrDefaultAsync(x => x.AccountId == accountId);
            if (nickname == null)
            {
                throw new Exception("Nickname not found");
            }
            return nickname;
        }
        private async Task CreateCurrencyInfo(long accountId)
        {
            var accountCurrencyEntity = new AccountCurrencyEntity
            {
                AccountId = accountId,
                Gold = 0 // 기본 값 설정
            };
            await _accountDbContext.AccountCurrency.AddAsync(accountCurrencyEntity);
            await _accountDbContext.SaveChangesAsync();
        }

        private async Task CreateCharacterInfo(long accountId)
        {
            var accountCharacterEntity = new AccountCharacterEntity
            {
                AccountId = accountId,
                AccountCharacter = 1 // 기본 값 설정
            };
            await _accountDbContext.AccountCharacter.AddAsync(accountCharacterEntity);
            await _accountDbContext.SaveChangesAsync();
        }

        public async Task<bool> CheckGoldAsync(long accountId, long itemGold)
        {
            var userGold = await _accountDbContext.AccountCurrency
                .Where(ac => ac.AccountId == accountId)
                .Select(ac => ac.Gold)
                .FirstOrDefaultAsync();

            return userGold >= itemGold;
        }

        public async Task<bool> BuyCharacterAsync(long accountId, CharacterStore character)
        {
            using var transaction = await _accountDbContext.Database.BeginTransactionAsync();

            try
            {
                // 계정의 캐릭터 목록에서 character_id가 일치하는 항목을 찾기
                var accountCharacter = await _accountDbContext.AccountCharacter
                    .SingleOrDefaultAsync(ac => ac.AccountId == accountId && ac.AccountCharacter == character.CharacterId);

                if (accountCharacter != null)
                {
                    // 이미 존재하는 캐릭터인 경우 예외 처리
                    throw new InvalidOperationException("Character already owned by the account.");
                }

                // 계정 캐릭터 목록에 새 캐릭터 추가
                _accountDbContext.AccountCharacter.Add(new AccountCharacterEntity()
                {
                    AccountId = accountId,
                    AccountCharacter = character.CharacterId
                });

                await _accountDbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                Logger.SetLogger(LOGTYPE.ERROR,ex.Message);
                await transaction.RollbackAsync();
                // 에러 처리 로직 추가 가능
                throw; // 필요시 에러를 다시 던질 수 있음
            }

            return true;
        }
        public async Task<(bool, List<RankRating>)> GetTop10RanksAsync()
        {
            try
            {
                var topRanks = await _accountDbContext.AccountRating
                    .OrderByDescending(ar => ar.RankRating)
                    .Take(10)
                    .Select(ar => new RankRating
                    {
                        AccountId = ar.AccountId,
                        Rating = ar.RankRating
                    })
                    .ToListAsync();

                return (true, topRanks);
            }
            catch (Exception ex)
            {
                Logger.SetLogger(LOGTYPE.ERROR, ex.Message);
                return (false, null);
            }
        }
       
        public async Task<(bool, int, int)> GetMyRating(long accountId)
        {
            try
            {
                var accountRating = await _accountDbContext.AccountRating
                    .AsNoTracking()
                    .SingleOrDefaultAsync(ar => ar.AccountId == accountId);

                if (accountRating != null)
                {
                    return (true, accountRating.NormalRating, accountRating.RankRating);
                }

                return (false, 0, 0);
            }
            catch (Exception ex)
            {
                Logger.SetLogger(LOGTYPE.ERROR, ex.Message + $" Error fetching rank rating for accountId {accountId}");
                return (false, 0, 0);
            }
        }

        public async Task<bool> SetMyRating(long accountId, int normalRating, int rankRaing)
        {
            using var transaction = await _accountDbContext.Database.BeginTransactionAsync();

            try
            {
                var accountRank = await _accountDbContext.AccountRating
                    .SingleOrDefaultAsync(ar => ar.AccountId == accountId);

                if (accountRank != null)
                {
                    accountRank.NormalRating = normalRating;
                    accountRank.RankRating = rankRaing;

                    _accountDbContext.AccountRating.Update(accountRank);
                    await _accountDbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return true;
                }

                return false; 
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Logger.SetLogger(LOGTYPE.ERROR, ex.Message + $" Error updating rank rating for accountId {accountId}");
                return false; 
            }
        }
        public async Task<(bool, long)> GetAccountGuildId(long accountId)
        {
            try
            {
                var accountGuild = await _accountDbContext.AccountGuild
                    .AsNoTracking()
                    .SingleOrDefaultAsync(ar => ar.AccountId == accountId);

                if (accountGuild != null)
                {
                    return (true, accountGuild.GuildId);
                }

                return (false, 0);
            }
            catch (Exception ex)
            {
                Logger.SetLogger(LOGTYPE.ERROR, ex.Message + $"Error fetching rank rating for accountId {accountId}");
                return (false, 0);
            }
        }
        public async Task<bool> SetAccountGuildId(long accountId, long guildId)
        {
            using var transaction = await _accountDbContext.Database.BeginTransactionAsync();

            try
            {
                var accountGuild = await _accountDbContext.AccountGuild
                    .SingleOrDefaultAsync(ar => ar.AccountId == accountId);

                if (accountGuild != null)
                {
                    accountGuild.GuildId = guildId;

                    _accountDbContext.AccountGuild.Update(accountGuild);
                    await _accountDbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Logger.SetLogger(LOGTYPE.ERROR, ex.Message + $"Error updating rank rating for accountId {accountId}");
                return false;
            }
        }
        public async Task<bool> CreateGuildAsync(long creatorId, string guildName)
        {
            using var transaction = await _accountDbContext.Database.BeginTransactionAsync();

            try
            {
                // 새로운 길드 엔터티 생성
                var guildEntity = new GuildEntity
                {
                    GuildName = guildName
                };

                // 길드를 데이터베이스에 추가
                await _accountDbContext.Guild.AddAsync(guildEntity);
                await _accountDbContext.SaveChangesAsync();

                // AccountGuild 엔티티 업데이트
                var accountGuildEntity = new AccountGuildEntity
                {
                    AccountId = creatorId,
                    GuildId = guildEntity.GuildId, // 자동 생성된 GuildId 사용
                    UserClass = (int)GUILDCLASS.Master // UserClass 초기 값 설정
                };

                // 기존의 AccountGuild 엔티티가 있는지 확인
                var existingAccountGuild = await _accountDbContext.AccountGuild
                    .FirstOrDefaultAsync(ag => ag.AccountId == creatorId);

                if (existingAccountGuild == null)
                {
                    // 기존 엔티티가 없으면 새로 추가
                    await _accountDbContext.AccountGuild.AddAsync(accountGuildEntity);
                }
                else
                {
                    // 기존 엔티티가 있으면 업데이트
                    existingAccountGuild.GuildId = guildEntity.GuildId;
                    existingAccountGuild.UserClass = (int)GUILDCLASS.Master;
                    _accountDbContext.AccountGuild.Update(existingAccountGuild);
                }

                await _accountDbContext.SaveChangesAsync();

                // 트랜잭션 커밋
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Logger.SetLogger(LOGTYPE.ERROR, ex.Message + $"Error creating guild with name {guildName}");
                return false;
            }
        }
        public async Task<GuildEntity> GetGuildIdAsync(long guildId)
        {
            var guild = await _accountDbContext.Guild.AsNoTracking().SingleOrDefaultAsync(x => x.GuildId == guildId);
            if (guild == null)
            {
                throw new Exception("Guild not found");
            }
            return guild;
        }

        public async Task<(bool, List<GuildInfo>)> FindGuildAsync(string guildName)
        {
            try
            {
                // 길드 이름으로 길드 정보를 검색합니다. GuildName이 null이 아닌 경우에만 검색
                if (guildName == null) return (false, new List<GuildInfo>());
                var guildEntities = await _accountDbContext.Guild
                    .Where(g => g.GuildName != null && g.GuildName.Contains(guildName))
                    .ToListAsync();

                // 검색된 길드 엔티티를 GuildInfo 객체로 변환합니다.
                var guildInfos = guildEntities.Select(g => new GuildInfo
                {
                    GuildUid = g.GuildId,
                    GuildName = g.GuildName
                }).ToList();

                // 검색 결과가 있을 경우 true 반환
                return (guildInfos.Any(), guildInfos);
            }
            catch (Exception ex)
            {
                Logger.SetLogger(LOGTYPE.ERROR, ex.Message + $"Error finding guild with name {guildName}");
                return (false, new List<GuildInfo>());
            }
        }


        public async Task<bool> SetGuildRequestUserAsync(long guildId, long accountId)
        {
            try
            {
                // 동일한 요청이 이미 있는지 확인
                var existingRequest = await _accountDbContext.GuildRequestUser
                    .FirstOrDefaultAsync(gru => gru.UserId == accountId && gru.GuildId == guildId);

                if (existingRequest != null)
                {
                    // 이미 요청이 존재하면 false 반환
                    return false;
                }

                // 새로운 요청 엔티티 생성
                var guildRequestUserEntity = new GuildRequestUserEntity
                {
                    UserId = accountId,
                    GuildId = guildId
                };

                // 요청 엔티티를 데이터베이스에 추가
                await _accountDbContext.GuildRequestUser.AddAsync(guildRequestUserEntity);
                await _accountDbContext.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting guild request user for accountId {AccountId} and guildId {GuildId}", accountId, guildId);
                return false;
            }
        }


        public async Task<(bool, string)> GetGuildInfoAsync(long guildId)
        {
            try
            {
                // 길드 정보 가져오기
                var guildEntity = await _accountDbContext.Guild
                    .FirstOrDefaultAsync(g => g.GuildId == guildId);

                if (guildEntity == null)
                {
                    return (false, null);
                }

                // 길드 크루 정보 가져오기
                var accountGuildEntities = await _accountDbContext.AccountGuild
                    .Where(ag => ag.GuildId == guildId)
                    .ToListAsync();

                var guildCrews = new List<GuildCrew>();
                foreach (var ag in accountGuildEntities)
                {
                    var nicknameEntity = await _accountDbContext.AccountNickName
                        .FirstOrDefaultAsync(a => a.AccountId == ag.AccountId);

                    var guildCrew = new GuildCrew
                    {
                        CrewUid = ag.AccountId,
                        CrewName = nicknameEntity.AccountNickName,
                        GuildClass = (GUILDCLASS)ag.UserClass
                    };

                    guildCrews.Add(guildCrew);
                }

                // MyGuildInfo 객체 생성
                var guildInfo = new MyGuildInfo
                {
                    GuildUid = guildEntity.GuildId,
                    GuildName = guildEntity.GuildName,
                    Crew = guildCrews
                };

                // MyGuildInfo 객체를 JSON 문자열로 변환
                var guildInfoJson = JsonConvert.SerializeObject(guildInfo);

                return (true, guildInfoJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving guild information for guildId {GuildId}", guildId);
                return (false, null);
            }
        }

        public async Task<(bool, string)> GetGuildJoinRequestAsync(long guildId)
        {
            try
            {
                // 길드 아이디로 가입 요청한 유저들의 정보를 가져오기
                var requestUserEntities = await _accountDbContext.GuildRequestUser
                    .Where(gru => gru.GuildId == guildId)
                    .ToListAsync();

                if (!requestUserEntities.Any())
                {
                    return (false, "No join requests found for the specified guild.");
                }

                var requestUserInfos = new List<RequestUserInfo>();
                foreach (var request in requestUserEntities)
                {
                    var nicknameEntity = await _accountDbContext.AccountNickName
                        .FirstOrDefaultAsync(a => a.AccountId == request.UserId);

                    var requestUserInfo = new RequestUserInfo
                    {
                        AccountId = request.UserId,
                        UserName = nicknameEntity.AccountNickName
                    };

                    requestUserInfos.Add(requestUserInfo);
                }

                // GuildJoinRequest 객체 생성 및 JSON 변환
                var guildJoinRequest = new GuildJoinRequest
                {
                    RequestUserInfos = requestUserInfos
                };

                var guildJoinRequestJson = JsonConvert.SerializeObject(guildJoinRequest);

                return (true, guildJoinRequestJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving guild join requests for guildId {GuildId}", guildId);
                return (false, "Error retrieving guild join requests.");
            }
        }

        public async Task<bool> GuildJoinOKAsync(long accountId, long guildId)
        {
            using var transaction = await _accountDbContext.Database.BeginTransactionAsync();

            try
            {
                // 가입 요청 데이터 삭제
                var requestEntity = await _accountDbContext.GuildRequestUser
                    .FirstOrDefaultAsync(gru => gru.UserId == accountId && gru.GuildId == guildId);

                if (requestEntity == null)
                {
                    return false;
                }

                _accountDbContext.GuildRequestUser.Remove(requestEntity);

                // AccountGuildEntity에 추가 또는 업데이트
                var existingAccountGuild = await _accountDbContext.AccountGuild
                    .FirstOrDefaultAsync(ag => ag.AccountId == accountId);

                if (existingAccountGuild == null)
                {
                    // 존재하지 않으면 새로 추가
                    var accountGuildEntity = new AccountGuildEntity
                    {
                        AccountId = accountId,
                        GuildId = guildId,
                        UserClass = (int)GUILDCLASS.Crew // 기본적으로 크루로 설정
                    };

                    await _accountDbContext.AccountGuild.AddAsync(accountGuildEntity);
                }
                else
                {
                    // 존재하면 업데이트
                    existingAccountGuild.GuildId = guildId;
                    existingAccountGuild.UserClass = (int)GUILDCLASS.Crew;
                    _accountDbContext.AccountGuild.Update(existingAccountGuild);
                }

                await _accountDbContext.SaveChangesAsync();

                // 트랜잭션 커밋
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error adding account to guild for accountId {AccountId} and guildId {GuildId}", accountId, guildId);
                return false;
            }
        }
        public async Task<bool> GuildResignAsync(long accountId)
        {
            using var transaction = await _accountDbContext.Database.BeginTransactionAsync();

            try
            {
                // AccountGuild 테이블에서 해당 accountId를 가진 레코드를 찾습니다.
                var accountGuildEntity = await _accountDbContext.AccountGuild
                    .FirstOrDefaultAsync(ag => ag.AccountId == accountId);

                if (accountGuildEntity == null)
                {
                    // 해당 accountId가 길드에 가입되어 있지 않은 경우 false 반환
                    return false;
                }

                // guild_id와 user_class 값을 -1로 업데이트
                accountGuildEntity.GuildId = -1;
                accountGuildEntity.UserClass = -1;
                _accountDbContext.AccountGuild.Update(accountGuildEntity);
                await _accountDbContext.SaveChangesAsync();

                // 트랜잭션 커밋
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error resigning from guild for accountId {AccountId}", accountId);
                return false;
            }
        }

        public async Task<(bool, List<long>)> GetAllGuildUsers(long guildId)
        {
            try
            {
                // AccountGuild 테이블에서 해당 guildId를 가진 모든 레코드의 accountId를 가져옵니다.
                var accountIds = await _accountDbContext.AccountGuild
                    .Where(ag => ag.GuildId == guildId)
                    .Select(ag => ag.AccountId)
                    .ToListAsync();

                if (!accountIds.Any())
                {
                    // 해당 guildId에 속한 유저가 없는 경우 false 반환
                    return (false, new List<long>());
                }

                // 유저가 있는 경우 true와 accountId 리스트 반환
                return (true, accountIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all guild users for guildId {GuildId}", guildId);
                return (false, new List<long>());
            }
        }
    }
}
