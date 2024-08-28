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
        builder.Services.AddScoped<AccountService, AccountService>();
        builder.Services.AddSingleton<DataManager, DataManager>();
        builder.Services.AddScoped<ShopService, ShopService>();
        builder.Services.AddScoped<InventoryService, InventoryService>();
        builder.Services.AddScoped<RankService, RankService>();
        builder.Services.AddScoped<GuildService, GuildService>();
        builder.Services.AddScoped<SessionService, SessionService>();
        builder.Services.AddScoped<SessionManager, SessionManager>();
        builder.Services.AddScoped<WebSocketChatService, WebSocketChatService>();
        builder.Services.AddScoped<WebSocketMatchService, WebSocketMatchService>();
        builder.Services.AddScoped<WebSocketLoginService, WebSocketLoginService>();

        // 리포지토리들을 싱글톤으로 등록
        builder.Services.AddScoped<IAccountRepository, AccountRepositoryFromMySql>();
        builder.Services.AddScoped<AccountDbContext, AccountDbContext>();

        // RedisContext 등록
        builder.Services.AddScoped<RedisContext>(provider =>
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string configFilePath = Path.Combine(currentDirectory, "Configuration", "DBconfig.txt");

            var ipAndPort = Utils.GetRedisDbConfig(configFilePath);
            var connectionString = $"{ipAndPort.Item1}:{ipAndPort.Item2}";

            return new RedisContext(connectionString);
        });

        // RedisRepository 등록
        builder.Services.AddScoped<IRedisRepository, RedisRepository>();

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

        app.MapControllers(); // 컨트롤러 매핑

        // WebSocket 처리 추가
        app.UseWebSockets();

        app.Use(async (context, next) =>
        {
            var chatService = context.RequestServices.GetRequiredService<WebSocketChatService>();
            var matchService = context.RequestServices.GetRequiredService<WebSocketMatchService>();
            var loginService = context.RequestServices.GetRequiredService<WebSocketLoginService>();

            WebSocketHandler.Configure(chatService, matchService, loginService);

            await next.Invoke();
        });

        app.MapWebSocketHandlers(); // WebSocket 핸들러 매핑

        // 서버 시작 시 IP와 포트 출력
        app.Lifetime.ApplicationStarted.Register(() =>
        {
            Logger.SetLogger(LOGTYPE.INFO, $"Server is running on http://127.0.0.1:5000");
        });

        Logger.LoggerInit();
        app.Run(); // 애플리케이션 실행
    }

}
