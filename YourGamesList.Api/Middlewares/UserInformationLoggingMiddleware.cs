using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Services.Auth;
using YourGamesList.Common.Logging;

namespace YourGamesList.Api.Middlewares;

public class UserInformationLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<UserInformationLoggingMiddleware> _logger;

    public UserInformationLoggingMiddleware(RequestDelegate next, ILogger<UserInformationLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var userId = context.User.FindFirst(JwtCustomClaimNames.UserId)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            using (_logger.BeginScope(new Dictionary<string, object> { [LogProperties.UserId] = userId }))
            {
                await _next(context);
                return;
            }
        }

        await _next(context);
    }
}