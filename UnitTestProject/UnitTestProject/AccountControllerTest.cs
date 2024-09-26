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
            // ������ ��ŷ
            _accountRepository = Substitute.For<IAccountRepository>();
            _redisRepository = Substitute.For<IRedisRepository>();
            _dataManager = Substitute.For<DataManager>();
            _logger = Substitute.For<ILogger<AccountService>>();

            // ���� AccountService �ν��Ͻ� ����
            _accountService = new AccountService(_logger, _accountRepository, _redisRepository, _dataManager);

            // AccountController ����
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

            // ��й�ȣ �ؽ� ����
            var hashedPassword = Utils.HashPassword(request.Password);
            var accountEntity = new AccountEntity
            {
                UserId = request.Id,
                UserPassword = hashedPassword,
                AccountId = 1  // ������ AccountId ���� 
            };

            // �г��� ������ ��ȯ�ϴ� ��ŷ
            var nickNameEntity = new AccountNickNameEntity
            {
                AccountId = accountEntity.AccountId,
                AccountNickName = "TestNickName"
            };

            // ��� ������ ��ȯ�ϴ� ��ŷ (���� ���� true�� ������ ��� ID ��ȯ)
            var guildId = (true, 12345L);  // Item1: ���� ����, Item2: ��� ID

            // Mocking the repository responses
            _accountRepository.GetAccountAsync(request.Id).Returns(accountEntity);
            _accountRepository.GetNickName(accountEntity.AccountId).Returns(nickNameEntity);
            _accountRepository.GetAccountGuildId(accountEntity.AccountId).Returns(guildId);

            // Redis���� ���� ������ ���������� ��ȯ�ǵ��� ����
            _redisRepository.SetSessionAsync(Arg.Any<long>(), Arg.Any<string>(), Arg.Any<TimeSpan>()).Returns(Task.FromResult(true));

            // Act
            var result = await _accountController.Login(request);

            // JSON�� ��ü�� ������ȭ
            UserEntity userEntity = JsonConvert.DeserializeObject<UserEntity>(result.Userentity);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Login Success", result.Message); // ������ �޽����� ���� �ʿ�
            Assert.NotNull(result.Userentity);
            Assert.Equal(request.Id, userEntity.Userid);  // ���� ID�� �ùٸ��� ��ȯ�Ǵ��� Ȯ��
            Assert.Equal("TestNickName", userEntity.UserName);  // �г����� �ùٸ��� ��ȯ�Ǵ��� Ȯ��
            Assert.Equal(12345L, userEntity.GuildUID);  // ��� ID�� �ùٸ��� ��ȯ�Ǵ��� Ȯ��
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
            Assert.Equal(null, result.Message); // ������ �޽����� ���� �ʿ�
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
            Assert.Equal("Nickname Create Success", result.Message); // ������ �޽����� ���� �ʿ�
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