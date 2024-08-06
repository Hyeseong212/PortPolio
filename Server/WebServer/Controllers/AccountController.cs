using Microsoft.AspNetCore.Mvc;
using SharedCode.Model.HttpCommand;
using WebServer.Service;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebServer.Model;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> _logger;
        private readonly AccountService _accountService;

        public AccountController(ILogger<AccountController> logger, AccountService accountService)
        {
            _logger = logger;
            _accountService = accountService;
        }

        [HttpPost]
        public async Task<AccountCreateResponse> Create([FromBody] AccountCreateRequest request)
        {
            var (isSuccess, message) = await _accountService.CreateAsync(request.Id, request.Password, request.NickName);
            return new AccountCreateResponse
            {
                IsSuccess = isSuccess,
                Message = message
            };
        }

        [HttpPost]
        public async Task<AccountLoginResponse> Login([FromBody] AccountLoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Id))
            {
                // Id가 null이거나 빈 문자열인 경우에 대한 처리
                return new AccountLoginResponse
                {
                    IsSuccess = false,
                    Message = "ID cannot be null or empty.",
                    Userentity = null
                };
            }
            if (string.IsNullOrEmpty(request.Password))
            {
                // Id가 null이거나 빈 문자열인 경우에 대한 처리
                return new AccountLoginResponse
                {
                    IsSuccess = false,
                    Message = "Password cannot be null or empty.",
                    Userentity = null
                };
            }
            var (isSuccess, message, userentity) = await _accountService.LoginAsync(request.Id, request.Password);
            return new AccountLoginResponse
            {
                IsSuccess = isSuccess,
                Message = message,
                Userentity = userentity
            };
        }
        [HttpPost]
        public async Task<AccountLogoutResponse> Logout([FromBody] AccountLogoutRequest request)
        {
            var (isSuccess, message) = await _accountService.LogoutAsync(request.AccountId);
            return new AccountLogoutResponse
            {
                IsSuccess = isSuccess,
                Message = message,
            };
        }
        [HttpPost]
        public async Task<AccountLoginResponse> ModifyNickName(AccountNickNameRequest request)
        {
            if (string.IsNullOrEmpty(request.NickName))
            {
                // Id가 null이거나 빈 문자열인 경우에 대한 처리
                return new AccountLoginResponse
                {
                    IsSuccess = false,
                    Message = "NickName cannot be null or empty.",
                };
            }
            var (isSuccess, message) = await _accountService.ModifyNickNameAsync(request.AccountId, request.NickName);
            return new AccountLoginResponse
            {
                IsSuccess = isSuccess,
                Message = message
            };
        }
    }
}
