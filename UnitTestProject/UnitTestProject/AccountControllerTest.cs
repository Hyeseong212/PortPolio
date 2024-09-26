using Microsoft.Extensions.Logging;
using NSubstitute;
using SharedCode.Model.HttpCommand;
using WebServer.Controllers;
using WebServer.Repository.Interface;
using WebServer.Service;
using Xunit;
using System.Threading.Tasks;
using SharedCode.Model;
using WebServer.Model.DbEntity;
using System.Text.Json;
using Newtonsoft.Json;
namespace UnitTestProject
{
    public class AccountControllerTest
    {
        private readonly AccountService _accountService;
        private readonly DataManager _dataManager;
        private readonly IAccountRepository _accountRepository;
        private readonly IRedisRepository _redisRepository;
        private readonly ILogger<AccountService> _logger;
        private readonly AccountController _accountController;

        public AccountControllerTest()
        {
            // 의존성 모킹
            _accountRepository = Substitute.For<IAccountRepository>();
            _redisRepository = Substitute.For<IRedisRepository>();
            _dataManager = Substitute.For<DataManager>();
            _logger = Substitute.For<ILogger<AccountService>>();

            // 실제 AccountService 인스턴스 생성
            _accountService = new AccountService(_logger, _accountRepository, _redisRepository, _dataManager);

            // AccountController 생성
            _accountController = new AccountController(Substitute.For<ILogger<AccountController>>(), _accountService);
        }

        [Fact]
        public async Task TestCreateAccount_Success()
        {
            // Arrange
            var request = new AccountCreateRequest { Id = "testId", Password = "ValidPassword123!", NickName = "TestNick" };

            _accountRepository.IsAlreadyExistAsync(request.Id).Returns(Task.FromResult(false));
            _accountRepository.CreateAccountAsync(request.Id, Arg.Any<string>(), request.NickName).Returns(Task.FromResult(true));

            // Act
            var result = await _accountController.Create(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Account Create Success", result.Message);
        }

        [Fact]
        public async Task TestCreateAccount_Failure()
        {
            // Arrange
            var request = new AccountCreateRequest { Id = "testId", Password = "short", NickName = "TestNick" };

            _accountRepository.IsAlreadyExistAsync(request.Id).Returns(Task.FromResult(false));

            // Act
            var result = await _accountController.Create(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Please create a password that is at least 10 characters long.", result.Message);
        }

        [Fact]
        public async Task TestLogin_Success()
        {
            // Arrange
            var request = new AccountLoginRequest { Id = "testId", Password = "ValidPassword123!" };

            // 비밀번호 해시 가정
            var hashedPassword = Utils.HashPassword(request.Password);
            var accountEntity = new AccountEntity
            {
                UserId = request.Id,
                UserPassword = hashedPassword,
                AccountId = 1  // 임의의 AccountId 설정 
            };

            // 닉네임 정보를 반환하는 모킹
            var nickNameEntity = new AccountNickNameEntity
            {
                AccountId = accountEntity.AccountId,
                AccountNickName = "TestNickName"
            };

            // 길드 정보를 반환하는 모킹 (성공 여부 true와 임의의 길드 ID 반환)
            var guildId = (true, 12345L);  // Item1: 성공 여부, Item2: 길드 ID

            // Mocking the repository responses
            _accountRepository.GetAccountAsync(request.Id).Returns(accountEntity);
            _accountRepository.GetNickName(accountEntity.AccountId).Returns(nickNameEntity);
            _accountRepository.GetAccountGuildId(accountEntity.AccountId).Returns(guildId);

            // Redis에서 세션 생성이 성공적으로 반환되도록 설정
            _redisRepository.SetSessionAsync(Arg.Any<long>(), Arg.Any<string>(), Arg.Any<TimeSpan>()).Returns(Task.FromResult(true));

            // Act
            var result = await _accountController.Login(request);

            // JSON을 객체로 역직렬화
            UserEntity userEntity = JsonConvert.DeserializeObject<UserEntity>(result.Userentity);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Login Success", result.Message); // 적절한 메시지로 수정 필요
            Assert.NotNull(result.Userentity);
            Assert.Equal(request.Id, userEntity.Userid);  // 유저 ID가 올바르게 반환되는지 확인
            Assert.Equal("TestNickName", userEntity.UserName);  // 닉네임이 올바르게 반환되는지 확인
            Assert.Equal(12345L, userEntity.GuildUID);  // 길드 ID가 올바르게 반환되는지 확인
        }


        [Fact]
        public async Task TestLogin_Failure_InvalidPassword()
        {
            // Arrange
            var request = new AccountLoginRequest { Id = "testId", Password = "WrongPassword" };

            _accountRepository.GetAccountAsync(request.Id).Returns(new AccountEntity { UserId = request.Id, UserPassword = "ValidPassword123!" });

            // Act
            var result = await _accountController.Login(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(null, result.Message); // 적절한 메시지로 수정 필요
            Assert.Null(result.Userentity);
        }

        [Fact]
        public async Task TestModifyNickName_Success()
        {
            // Arrange
            var request = new AccountNickNameRequest { AccountId = 1, NickName = "NewNickName" };

            _accountRepository.ModifyNickName(request.AccountId, request.NickName).Returns(Task.FromResult(true));

            // Act
            var result = await _accountController.ModifyNickName(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Nickname Create Success", result.Message); // 적절한 메시지로 수정 필요
        }

        [Fact]
        public async Task TestModifyNickName_Failure_EmptyNickName()
        {
            // Arrange
            var request = new AccountNickNameRequest { AccountId = 1, NickName = "" };

            // Act
            var result = await _accountController.ModifyNickName(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("NickName cannot be null or empty.", result.Message);
        }

        [Fact]
        public async Task TestLogout_Success()
        {
            // Arrange
            var request = new AccountLogoutRequest { AccountId = 1 };

            _redisRepository.DeleteSessionAsync(request.AccountId).Returns(Task.FromResult(true));

            // Act
            var result = await _accountController.Logout(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Logout Success", result.Message);
        }

        [Fact]
        public async Task TestLogout_Failure()
        {
            // Arrange
            var request = new AccountLogoutRequest { AccountId = 1 };

            _redisRepository.DeleteSessionAsync(request.AccountId).Returns(Task.FromResult(false));

            // Act
            var result = await _accountController.Logout(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Logout Fail", result.Message);
        }
    }
}