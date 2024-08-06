using WebServer.Repository.Interface;

namespace WebServer.Service
{
    public class InventoryService
    {
        private readonly ILogger<InventoryService> _logger;
        private readonly IAccountRepository _accountRepository; // 추가

        public InventoryService(ILogger<InventoryService> logger, IAccountRepository accountRepository)
        {
            _logger = logger;
            _accountRepository = accountRepository; // 초기화
        }
    }
}
