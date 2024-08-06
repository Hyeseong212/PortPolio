

using WebServer.Repository.Interface;

namespace WebServer.Repository
{
    public class AccountRepositoryFromMemory : IAccountRepository
    {
        Dictionary<string, AccountInfo> _idToAccountInfo;
        private readonly ILogger<AccountRepositoryFromMemory> _logger;

        public AccountRepositoryFromMemory(ILogger<AccountRepositoryFromMemory> logger)
        {
            _idToAccountInfo = new Dictionary<string, AccountInfo>();
            _logger = logger;
        }


        public bool IsAlreadyExist(string id)
        {
            return _idToAccountInfo.ContainsKey(id);
        }

        public async Task<bool> CreateAsync(string id, string pw)
        {
            AccountInfo accountInfo = new AccountInfo();
            accountInfo.Id = id;
            accountInfo.Pw = pw;
            _idToAccountInfo.Add(id, accountInfo);

            return true;
        }

        public bool Login(string id, string password)
        {
            if (!_idToAccountInfo.TryGetValue(id, out AccountInfo accountInfo))
            {
                return false;
            }

            if (accountInfo.Pw != password)
            { 
                return false;
            }

            return true;
        }
    }

    public class AccountInfo
    {
        public string Id { get; set; }
        public string Pw { get; set; }
    }
}
