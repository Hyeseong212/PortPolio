using Microsoft.Extensions.Logging;
using NSubstitute;
using SharedCode.Model.HttpCommand;
using WebServer.Controllers;
using WebServer.Repository.Interface;
using WebServer.Service;
using SharedCode.Model;
using Newtonsoft.Json;

namespace UnitTestProject
{
    public class GuildControllerTest
    {
        private readonly GuildService _guildService;
        private readonly IAccountRepository _accountRepository;
        private readonly ILogger<GuildService> _logger;
        private readonly GuildController _guildController;

        public GuildControllerTest()
        {
            // 의존성 모킹
            _accountRepository = Substitute.For<IAccountRepository>();
            _logger = Substitute.For<ILogger<GuildService>>();

            // 실제 AccountService 인스턴스 생성
            _guildService = new GuildService(_logger, _accountRepository);

            // AccountController 생성
            _guildController = new GuildController(Substitute.For<ILogger<GuildController>>(), _guildService);
        }
        [Fact]
        public async Task TestGuildCreate_Success()
        {
            // Arrange
            var request = new GuildCreateRequest { Creator = 1, GuildName = "TestGuild" };

            // Guild 생성이 성공하도록 모킹
            _accountRepository.CreateGuildAsync(request.Creator, request.GuildName).Returns(Task.FromResult(true));

            // Act
            var result = await _guildController.GuildCreate(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Guild Create Success", result.Message);
        }

        [Fact]
        public async Task TestGuildCreate_Failure()
        {
            // Arrange
            var request = new GuildCreateRequest { Creator = 1, GuildName = "TestGuild" };

            // Guild 생성이 실패하도록 모킹
            _accountRepository.CreateGuildAsync(request.Creator, request.GuildName).Returns(Task.FromResult(false));

            // Act
            var result = await _guildController.GuildCreate(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Guild Create Fail", result.Message);
        }
        [Fact]
        public async Task TestGuildFind_Success()
        {
            // Arrange
            var request = new FindGuildRequest { GuildName = "TestGuild" };
            var guildList = new List<GuildInfo>
    {
        new GuildInfo { GuildUid = 12345, GuildName = "TestGuild" }
    };

            // 길드 검색이 성공적으로 결과를 반환하도록 설정
            _accountRepository.FindGuildAsync(request.GuildName).Returns(Task.FromResult((true, guildList)));

            // Act
            var result = await _guildController.GuildFind(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Find Guild Success", result.Message);
            Assert.NotNull(result.GuildInfo);
            var guildInfo = JsonConvert.DeserializeObject<List<GuildInfo>>(result.GuildInfo);
            Assert.Single(guildInfo);
            Assert.Equal("TestGuild", guildInfo[0].GuildName);
        }

        [Fact]
        public async Task TestGuildFind_Failure()
        {
            // Arrange
            var request = new FindGuildRequest { GuildName = "TestGuild" };

            // 길드 검색이 실패하도록 설정
            _accountRepository.FindGuildAsync(request.GuildName).Returns(Task.FromResult((false, new List<GuildInfo>())));

            // Act
            var result = await _guildController.GuildFind(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Find Guild Fail", result.Message);
        }
        [Fact]
        public async Task TestRequestingJoinGuild_Success()
        {
            // Arrange
            var request = new RequestingJoinGuildRequest { GuildId = 1, AccountId = 12345 };

            // 길드 가입 요청이 성공하도록 설정
            _accountRepository.SetGuildRequestUserAsync(request.GuildId, request.AccountId).Returns(Task.FromResult(true));

            // Act
            var result = await _guildController.RequestingJoinGuild(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Join Request Success", result.Message);
        }

        [Fact]
        public async Task TestRequestingJoinGuild_Failure()
        {
            // Arrange
            var request = new RequestingJoinGuildRequest { GuildId = 1, AccountId = 12345 };

            // 길드 가입 요청이 실패하도록 설정
            _accountRepository.SetGuildRequestUserAsync(request.GuildId, request.AccountId).Returns(Task.FromResult(false));

            // Act
            var result = await _guildController.RequestingJoinGuild(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Join Request Fail", result.Message);
        }
        [Fact]
        public async Task TestGetGuildInfo_Success()
        {
            // Arrange
            var request = new GetGuildInfoRequest { GuildId = 1 };
            var guildInfo = new MyGuildInfo
            {
                GuildUid = 1,
                GuildName = "TestGuild",
                Crew = new List<GuildCrew>
        {
            new GuildCrew { CrewUid = 123, CrewName = "Member1" }
        }
            };

            // 길드 정보 요청이 성공적으로 처리되도록 설정
            _accountRepository.GetGuildInfoAsync(request.GuildId).Returns(Task.FromResult((true, JsonConvert.SerializeObject(guildInfo))));

            // Act
            var result = await _guildController.GetGuildInfo(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("GetGuildInfo Request Success", result.Message);
            Assert.NotNull(result.MyGuildInfo);
            var deserializedGuildInfo = JsonConvert.DeserializeObject<MyGuildInfo>(result.MyGuildInfo);
            Assert.Equal("TestGuild", deserializedGuildInfo.GuildName);
        }

        [Fact]
        public async Task TestGetGuildInfo_Failure()
        {
            // Arrange
            var request = new GetGuildInfoRequest { GuildId = 1 };

            // 길드 정보 요청이 실패하도록 설정
            _accountRepository.GetGuildInfoAsync(request.GuildId).Returns(Task.FromResult((false, "")));

            // Act
            var result = await _guildController.GetGuildInfo(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("GetGuildInfo Request Fail", result.Message);
        }

    }
}