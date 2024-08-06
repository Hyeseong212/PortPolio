using WebServer.Repository.Interface;
using SharedCode.Model;

namespace WebServer.Service
{
    public class ShopService
    {
        private readonly ILogger<ShopService> _logger;
        private readonly IAccountRepository _accountRepository; // 추가
        private readonly DataManager _dataManager;

        public ShopService(ILogger<ShopService> logger, IAccountRepository accountRepository, DataManager dataManager)
        {
            _logger = logger;
            _accountRepository = accountRepository; // 초기화
            _dataManager = dataManager;
        }
        public async Task<(bool, string)> BuyCharacter(long accountId, int itemId)
        {
            var item = _dataManager.CharacterDataHandler.GetItemById(itemId);
            if (item == null)
            {
                return (false, _dataManager.MessageHandler.GetMessage("Error", 7));
            }

            bool hasEnoughGold = await _accountRepository.CheckGoldAsync(accountId, item.Price);
            if (!hasEnoughGold)
            {
                return (false, _dataManager.MessageHandler.GetMessage("Error", 6));
            }

            // 아이템 구매 로직 (예: 골드 차감, 인벤토리에 아이템 추가 등)
            var userGold = await _accountRepository.GetGoldAsync(accountId);

            var isCharacterUpdateSuccess = false;
            var isGoldSuccess = false;
            if (item.Price <= userGold) //유저의 골드가 충분히있을때
            {
                isCharacterUpdateSuccess = await _accountRepository.BuyCharacterAsync(accountId, item);
                isGoldSuccess = await _accountRepository.UpdateGoldAsync(accountId, userGold - item.Price);
            }
            else
            {
                return (false, _dataManager.MessageHandler.GetMessage("Error", 6));
            }



            if (isGoldSuccess && isCharacterUpdateSuccess)
            {
                return (true, _dataManager.MessageHandler.GetMessage("Info", 3));
            }
            else
            {
                return (false, _dataManager.MessageHandler.GetMessage("Error", 9));

            }
        }
        public async Task<(bool, string)> BuyItem(long accountId, int itemId, int count)
        {
            var item = _dataManager.ItemHandler.GetItemById(itemId);

            bool hasEnoughGold = await _accountRepository.CheckGoldAsync(accountId, item.Price);
            if (!hasEnoughGold)
            {
                return (false, _dataManager.MessageHandler.GetMessage("Error", 6));
            }

            // 아이템 구매 로직 (예: 골드 차감, 인벤토리에 아이템 추가 등)
            var userGold = await _accountRepository.GetGoldAsync(accountId);

            var isItemUpdateSuccess = false;
            var isGoldSuccess = false;
            if (item.Price * (long)count <= userGold) //유저의 골드가 충분히있을때
            {
                isItemUpdateSuccess = await _accountRepository.BuyItemAsync(accountId, item, count);
                isGoldSuccess = await _accountRepository.UpdateGoldAsync(accountId, userGold - item.Price);
            }
            else
            {
                return (false, _dataManager.MessageHandler.GetMessage("Error", 9));
            }



            if (isGoldSuccess && isItemUpdateSuccess)
            {
                return (true, _dataManager.MessageHandler.GetMessage("Info", 3));
            }
            else
            {
                return (false, _dataManager.MessageHandler.GetMessage("Error", 9));

            }
        }
        public async Task<(bool, string)> SellItem(long accountId, int itemId, int count)
        {
            var item = _dataManager.ItemHandler.GetItemById(itemId);
            if (item == null) return (false, _dataManager.MessageHandler.GetMessage("Error", 10));

            var isItemUpdateSuccess = await _accountRepository.SellItemAsync(accountId, item, count);

            var isGoldSuccess = false;

            if (isItemUpdateSuccess)
            {
                var userGold = await _accountRepository.GetGoldAsync(accountId);
                long updatedPrice = (long)((float)item.Price * 0.25f);
                isGoldSuccess = await _accountRepository.UpdateGoldAsync(accountId, userGold + updatedPrice);
            }
            else
            {
                return (false, _dataManager.MessageHandler.GetMessage("Error", 10));
            }


            if (isGoldSuccess)
            {
                return (true, _dataManager.MessageHandler.GetMessage("Info", 4));
            }
            else
            {
                return (false, _dataManager.MessageHandler.GetMessage("Error", 10));
            }
        }
    }
}