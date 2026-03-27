using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Logging;
using NSubstitute;
using YourGamesList.Api.OutputCachePolicies;
using YourGamesList.TestsUtils;

namespace YourGamesList.Api.UnitTests.OutputCachePolicies;

public class AlwaysOnOkOutputPolicyTests
{
    private ILogger<AlwaysOnOkOutputPolicy> _logger;

    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger<AlwaysOnOkOutputPolicy>>();
    }

    [Test]
    public void CacheRequestAsync_OnSuccess_ReturnsCompletedTask()
    {
        //ARRANGE
        var cancellationToken = CancellationToken.None;
        var httpContext = new DefaultHttpContext();
        var context = new OutputCacheContext
        {
            HttpContext = httpContext,
            EnableOutputCaching = false,
            AllowCacheLookup = false,
            AllowCacheStorage = false,
            AllowLocking = false
        };

        var policy = new AlwaysOnOkOutputPolicy(_logger);

        //ACT
        var res = policy.CacheRequestAsync(context, cancellationToken);

        //ASSERT
        Assert.That(res, Is.EqualTo(ValueTask.CompletedTask));
        Assert.That(context.EnableOutputCaching, Is.True);
        Assert.That(context.AllowCacheLookup, Is.True);
        Assert.That(context.AllowCacheStorage, Is.True);
        Assert.That(context.AllowLocking, Is.True);
        Assert.That(context.ResponseExpirationTimeSpan, Is.EqualTo(TimeSpan.FromHours(1)));
    }


    [Test]
    public void ServeFromCacheAsync_OnSuccess_ReturnsCompletedTask()
    {
        //ARRANGE
        var cancellationToken = CancellationToken.None;
        var httpContext = new DefaultHttpContext();
        var context = new OutputCacheContext
        {
            HttpContext = httpContext,
        };

        var policy = new AlwaysOnOkOutputPolicy(_logger);

        //ACT
        var res = policy.ServeFromCacheAsync(context, cancellationToken);

        //ASSERT
        Assert.That(res, Is.EqualTo(ValueTask.CompletedTask));
        _logger.ReceivedLog(LogLevel.Information, $"Serving from cache for request to '{context.HttpContext.Request.Path.Value}'. ");
    }


    [Test]
    public void ServeResponseAsync_On200OK_SavesInCache()
    {
        //ARRANGE
        var cancellationToken = CancellationToken.None;
        var httpContext = new DefaultHttpContext();
        httpContext.Response.StatusCode = 200;
        var context = new OutputCacheContext
        {
            HttpContext = httpContext,
            AllowCacheStorage = false
        };

        var policy = new AlwaysOnOkOutputPolicy(_logger);

        //ACT
        var res = policy.ServeResponseAsync(context, cancellationToken);

        //ASSERT
        Assert.That(res, Is.EqualTo(ValueTask.CompletedTask));
        Assert.That(context.AllowCacheStorage, Is.True);
        _logger.ReceivedLog(LogLevel.Information, ["Response status code is 200 OK. Allowing cache storage. Will store cache for"]);
    }

    [Test]
    public void ServeResponseAsync_OnNonSuccessStatusCode_SavesInCache()
    {
        //ARRANGE
        var cancellationToken = CancellationToken.None;
        var httpContext = new DefaultHttpContext();
        httpContext.Response.StatusCode = 400;
        var context = new OutputCacheContext
        {
            HttpContext = httpContext,
            AllowCacheStorage = true
        };

        var policy = new AlwaysOnOkOutputPolicy(_logger);

        //ACT
        var res = policy.ServeResponseAsync(context, cancellationToken);

        //ASSERT
        Assert.That(res, Is.EqualTo(ValueTask.CompletedTask));
        Assert.That(context.AllowCacheStorage, Is.False);
        _logger.ReceivedLog(LogLevel.Information, "Response status code is not 200 OK. Disabling cache storage.");
    }
}