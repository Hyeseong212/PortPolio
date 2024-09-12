using Moq;
using System.Security.Principal;
using WebServer.Controllers;
using WebServer.Service;

namespace UnitTestProject
{
    public class ControllerTest
    {
        [Fact]
        public void TestGetAccount()
        {
            // ���� ��ŷ (��: IAccountService)
            var mockAccountService = new Mock<AccountService>();
            mockAccountService.Setup(service => service.GetAccount(It.IsAny<int>()))
                              .Returns(new Account { Id = 1, Name = "Test Account" });

            // ��Ʈ�ѷ� �ν��Ͻ� ����
            var accountController = new AccountController(mockAccountService.Object);

            // �׽�Ʈ ����
            var result = accountController.GetAccount(1);

            // ����
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Test Account", result.Name);
        }
    }
}