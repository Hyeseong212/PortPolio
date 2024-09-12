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
            // 서비스 모킹 (예: IAccountService)
            var mockAccountService = new Mock<AccountService>();
            mockAccountService.Setup(service => service.GetAccount(It.IsAny<int>()))
                              .Returns(new Account { Id = 1, Name = "Test Account" });

            // 컨트롤러 인스턴스 생성
            var accountController = new AccountController(mockAccountService.Object);

            // 테스트 실행
            var result = accountController.GetAccount(1);

            // 검증
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Test Account", result.Name);
        }
    }
}