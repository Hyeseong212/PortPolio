using Microsoft.AspNetCore.Mvc;
using SharedCode.Model.HttpCommand;
using WebServer.Service;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class ShopController : ControllerBase
    {
        private readonly ILogger<ShopController> _logger;
        private readonly ShopService _shopService;

        public ShopController(ILogger<ShopController> logger, ShopService shopService)
        {
            _logger = logger;
            _shopService = shopService;
        }

        [HttpPost]
        public async Task<ShopBuyCharacterResponse> BuyCharacter([FromBody] ShopBuyCharacterRequest request)
        {
            var (isSuccess, message) = await _shopService.BuyCharacter(request.AccountId, request.ItemId);
            return new ShopBuyCharacterResponse
            {
                IsSuccess = isSuccess,
                Message = message
            };
        }
        [HttpPost]
        public async Task<ShopBuyItemResponse> BuyItem([FromBody] ShopBuyItemRequest request)
        {
            var (isSuccess, message) = await _shopService.BuyItem(request.AccountId, request.ItemId, request.Count);
            return new ShopBuyItemResponse
            {
                IsSuccess = isSuccess,
                Message = message
            };
        }
        [HttpPost]
        public async Task<ShopSellItemResponse> SellItem([FromBody] ShopSellItemRequest request)
        {
            var (isSuccess, message) = await _shopService.SellItem(request.AccountId, request.ItemId, request.Count);
            return new ShopSellItemResponse
            {
                IsSuccess = isSuccess,
                Message = message
            };
        }
    }
}
