using Microsoft.AspNetCore.Mvc;
using SharedCode.Model.HttpCommand;
using WebServer.Service;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class InventoryController : ControllerBase
    {
        private readonly ILogger<InventoryController> _logger;
        private readonly ShopService _shopService;

        public InventoryController(ILogger<InventoryController> logger, ShopService shopService)
        {
            _logger = logger;
            _shopService = shopService;
        }

    }
}
