using System.Collections.Generic;
using System.Security.Claims;
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
        var userId = GetClaimOrNotAvailable(context.User, JwtCustomClaimNames.UserId);

        var scopes = new Dictionary<string, object>
        {
            [LogProperties.UserId] = userId
        };

        using (_logger.BeginScope(scopes))
        {
            await _next(context);
        }
    }

    private static string GetClaimOrNotAvailable(ClaimsPrincipal user, string claimType)
    {
        var value = user.FindFirst(claimType)?.Value;
        return !string.IsNullOrEmpty(value) ? value : "N/A";
    }
}