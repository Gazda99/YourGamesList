using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using YourGamesList.Api.Middlewares;
using YourGamesList.Api.Services.Auth;
using YourGamesList.Common.Logging;
using YourGamesList.TestsUtils;

namespace YourGamesList.Api.UnitTests.Middlewares;

public class UserInformationLoggingMiddlewareTests
{
    private RequestDelegate _next;
    private ILogger<UserInformationLoggingMiddleware> _logger;
    private UserInformationLoggingMiddleware _middleware;

    [SetUp]
    public void Setup()
    {
        _next = Substitute.For<RequestDelegate>();
        _logger = Substitute.For<ILogger<UserInformationLoggingMiddleware>>();
        _middleware = new UserInformationLoggingMiddleware(_next, _logger);
    }

    [Test]
    public async Task InvokeAsync_LogsAll()
    {
        //ARRANGE
        const string expectedUserId = "a0b1c2d3-e4f5-6789-0123-456789abcdef";
        var claims = new[]
        {
            new Claim(JwtCustomClaimNames.UserId, expectedUserId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };

        var scopeDisposable = Substitute.For<IDisposable>();
        _logger.BeginScope(Arg.Any<Dictionary<string, object>>()).Returns(scopeDisposable);

        //ACT
        await _middleware.InvokeAsync(httpContext);

        //ASSERT
        _logger.ReceivedBeginScope(new Dictionary<string, object>()
        {
            { LogProperties.UserId, expectedUserId }
        });
        await _next.Received(1).Invoke(httpContext);
        scopeDisposable.Received(1).Dispose();
    }

    [Test]
    public async Task InvokeAsync_WhenUserIdClaimIsNullOrEmpty_LogsNA()
    {
        //ARRANGE
        var claims = new[]
        {
            new Claim(JwtCustomClaimNames.UserId, string.Empty)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };

        var scopeDisposable = Substitute.For<IDisposable>();
        _logger.BeginScope(Arg.Any<Dictionary<string, object>>()).Returns(scopeDisposable);

        //ACT
        await _middleware.InvokeAsync(httpContext);

        //ASSERT
        _logger.ReceivedBeginScope(new Dictionary<string, object>()
        {
            { LogProperties.UserId, "N/A" }
        });

        await _next.Received(1).Invoke(httpContext);
        scopeDisposable.Received(1).Dispose();
    }
}