using Microsoft.Extensions.Logging;
using NSubstitute;
using SharedCode.Model.HttpCommand;
using WebServer.Controllers;
using WebServer.Repository.Interface;
using WebServer.Service;
using Xunit;
using System.Threading.Tasks;

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

        // ID가 존재하지 않는 것으로 설정
        _accountRepository.IsAlreadyExistAsync(request.Id).Returns(Task.FromResult(false));

        // 비밀번호 유효성 검사를 통과하도록 설정
        // 필요하면 추가적인 의존성 모킹

        // AccountRepository.CreateAccountAsync가 성공적으로 계정을 생성하도록 설정
        _accountRepository.CreateAccountAsync(request.Id, Arg.Any<string>(), request.NickName)
                          .Returns(Task.FromResult(true));

        // Act
        var result = await _accountController.Create(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Account Create Success", result.Message); // 실제 메시지로 수정 필요
    }

    [Fact]
    public async Task TestCreateAccount_Failure()
    {
        // Arrange
        var request = new AccountCreateRequest { Id = "testId", Password = "short", NickName = "TestNick" };

        // 비밀번호가 너무 짧은 경우
        // ID가 존재하지 않는 것으로 설정
        _accountRepository.IsAlreadyExistAsync(request.Id).Returns(Task.FromResult(false));

        // Act
        var result = await _accountController.Create(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Please create a password that is at least 10 characters long.", result.Message); // 실제 메시지로 수정 필요
    }

    // 나머지 테스트 메서드들도 동일한 방식으로 수정
}
