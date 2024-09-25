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

        // ID�� �������� �ʴ� ������ ����
        _accountRepository.IsAlreadyExistAsync(request.Id).Returns(Task.FromResult(false));

        // ��й�ȣ ��ȿ�� �˻縦 ����ϵ��� ����
        // �ʿ��ϸ� �߰����� ������ ��ŷ

        // AccountRepository.CreateAccountAsync�� ���������� ������ �����ϵ��� ����
        _accountRepository.CreateAccountAsync(request.Id, Arg.Any<string>(), request.NickName)
                          .Returns(Task.FromResult(true));

        // Act
        var result = await _accountController.Create(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Account Create Success", result.Message); // ���� �޽����� ���� �ʿ�
    }

    [Fact]
    public async Task TestCreateAccount_Failure()
    {
        // Arrange
        var request = new AccountCreateRequest { Id = "testId", Password = "short", NickName = "TestNick" };

        // ��й�ȣ�� �ʹ� ª�� ���
        // ID�� �������� �ʴ� ������ ����
        _accountRepository.IsAlreadyExistAsync(request.Id).Returns(Task.FromResult(false));

        // Act
        var result = await _accountController.Create(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Please create a password that is at least 10 characters long.", result.Message); // ���� �޽����� ���� �ʿ�
    }

    // ������ �׽�Ʈ �޼���鵵 ������ ������� ����
}
