using WebServer.Repository;
using WebServer.Repository.Interface;
using WebServer.Service;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.WebSockets;
public class Initializer
{

    public Initializer() 
    { 
    }
    public void Init(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 컨트롤러 서비스를 컨테이너에 추가
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "WebServer API", Version = "v1" });
        });

        // 서비스들을 싱글톤으로 등록
        builder.Services.AddSingleton<AccountService, AccountService>();
        builder.Services.AddSingleton<DataManager, DataManager>();
        builder.Services.AddSingleton<ShopService, ShopService>();
        builder.Services.AddSingleton<InventoryService, InventoryService>();
        builder.Services.AddSingleton<RankService, RankService>();
        builder.Services.AddSingleton<GuildService, GuildService>();
        builder.Services.AddSingleton<SessionService, SessionService>();
        builder.Services.AddSingleton<SessionManager, SessionManager>();
        builder.Services.AddSingleton<WebSocketChatService, WebSocketChatService>();
        builder.Services.AddSingleton<WebSocketMatchService, WebSocketMatchService>();
        builder.Services.AddSingleton<WebSocketLoginService, WebSocketLoginService>();

        // 리포지토리들을 싱글톤으로 등록
        builder.Services.AddSingleton<IAccountRepository, AccountRepositoryFromMySql>();
        builder.Services.AddSingleton<AccountDbContext, AccountDbContext>();

        // RedisContext 등록
        builder.Services.AddSingleton<RedisContext>(provider =>
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string configFilePath = Path.Combine(currentDirectory, "Configuration", "DBconfig.txt");

            var ipAndPort = Utils.GetRedisDbConfig(configFilePath);
            var connectionString = $"{ipAndPort.Item1}:{ipAndPort.Item2}";

            return new RedisContext(connectionString);
        });

        // RedisRepository 등록
        builder.Services.AddSingleton<IRedisRepository, RedisRepository>();

        // WebSocket 서비스 추가
        builder.Services.AddWebSockets(options =>
        {
            options.KeepAliveInterval = TimeSpan.FromMinutes(2);
        });

        // Kestrel 설정: HTTP 포트를 5000으로 고정
        builder.WebHost.ConfigureKestrel(serverOptions =>
        {
            serverOptions.ListenAnyIP(5000); // 모든 IP 주소에서 포트 5000으로 리스닝
        });

        AppInit(builder.Build());
    }

    public void AppInit(WebApplication app)
    {
        // HTTP 요청 파이프라인 구성
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage(); // 개발 환경에서 예외 페이지 활성화
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebServer API v1");
                c.RoutePrefix = string.Empty; // Swagger UI가 루트 URL에서 제공되도록 설정
            });
        }

        app.UseRouting(); // 라우팅 미들웨어 추가
        app.UseAuthorization(); // 권한 부여 미들웨어 사용
        //app.UseEndpoints(endpoints =>
        //{
        //    endpoints.MapControllers(); // 컨트롤러 매핑
        //});
        // 최상위 라우트 등록 사용
        app.MapControllers(); // 컨트롤러 매핑

        // WebSocket 처리 추가
        app.UseWebSockets();

        var chatService = app.Services.GetRequiredService<WebSocketChatService>();
        var matchService = app.Services.GetRequiredService<WebSocketMatchService>();
        var loginService = app.Services.GetRequiredService<WebSocketLoginService>(); // WS_LoginService 가져오기
        WebSocketHandler.Configure(chatService, matchService, loginService); // loginService를 포함하여 설정

        app.MapWebSocketHandlers(); // WebSocket 핸들러 매핑

        // 서버 시작 시 IP와 포트 출력
        app.Lifetime.ApplicationStarted.Register(() =>
        {
            Console.WriteLine($"Server is running on http://127.0.0.1:5000");
        });

        app.Run(); // 애플리케이션 실행
    }

}
