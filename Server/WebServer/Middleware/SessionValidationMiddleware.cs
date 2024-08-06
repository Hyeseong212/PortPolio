using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using WebServer.Service;

public class SessionValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly SessionService _sessionService;

    public SessionValidationMiddleware(RequestDelegate next, SessionService sessionService)
    {
        _next = next;
        _sessionService = sessionService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 로그인 및 세션 생성 엔드포인트는 예외 처리
        if (context.Request.Path.StartsWithSegments("/Account/Login") ||
            context.Request.Path.StartsWithSegments("/Account/Create"))
        {
            await _next(context);
            return;
        }

        if (context.Request.Headers.TryGetValue("SessionId", out var sessionId) &&
            context.Request.Headers.TryGetValue("AccountId", out var accountIdString) &&
            long.TryParse(accountIdString, out var accountId))
        {

            var isValid = await _sessionService.ValidateSessionAsync(accountId, sessionId);
            if (!isValid)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid session");
                return;
            }
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("SessionId and AccountId headers are required");
            return;
        }

        await _next(context);
    }
}
