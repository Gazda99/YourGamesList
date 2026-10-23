using System;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using YourGamesList.Api.Services.Twitch;
using YourGamesList.Api.Services.Twitch.Model;
using YourGamesList.Common;
using YourGamesList.Common.Caching;
using YourGamesList.TestsUtils;

namespace YourGamesList.Api.UnitTests.Services.Twitch;

public class TwitchAuthServiceWithCachingTests
{
    private const string TwitchAuthTokenCacheKey = "YGL_twitch-auth-token";
    private const string TwitchAuthLockKey = "YGL_twitch-auth-lock";
    private const int TwitchAuthTokenCacheDurationInHours = 48;
    private const int TwitchAuthLockDurationInSeconds = 60;

    private IFixture _fixture;
    private ILogger<TwitchAuthServiceWithCaching> _logger;
    private ICacheProvider _cacheProvider;
    private ITwitchAuthService _innerService;
    private TwitchAuthServiceWithCaching _service;
    private FakeTimeProvider _timeProvider;
    private IAsyncDisposable _lockLease;
    

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _logger = Substitute.For<ILogger<TwitchAuthServiceWithCaching>>();
        _cacheProvider = Substitute.For<ICacheProvider>();
        _innerService = Substitute.For<ITwitchAuthService>();
        _timeProvider = new FakeTimeProvider();
        _lockLease = Substitute.For<IAsyncDisposable>();
        _lockLease.DisposeAsync().Returns(ValueTask.CompletedTask);
        _cacheProvider.AcquireLock(
                TwitchAuthLockKey,
                TimeSpan.FromSeconds(TwitchAuthLockDurationInSeconds),
                Arg.Any<System.Threading.CancellationToken>())
            .Returns(Task.FromResult(_lockLease));
        _service = new TwitchAuthServiceWithCaching(_logger, _cacheProvider, _innerService, _timeProvider);
    }

    [Test]
    public void GetClientId_ShouldJustCallInnerGetClientId()
    {
        //ARRANGE
        var clientId = _fixture.Create<string>();
        _innerService.GetClientId().Returns(clientId);

        //ACT
        var res = _service.GetClientId();

        //ASSERT
        Assert.That(res, Is.EquivalentTo(clientId));
        _innerService.Received(1).GetClientId();
    }

    [Test]
    public async Task GetAccessToken_ShouldReturnCachedToken_IfExistsInCache()
    {
        //ARRANGE
        var cachedToken = _fixture.Build<TwitchToken>()
            .With(x => x.ExpiresOn, _timeProvider.GetUtcNow() + TimeSpan.FromHours(1))
            .Create();
        _cacheProvider.Get<TwitchToken>(TwitchAuthTokenCacheKey, Arg.Any<System.Threading.CancellationToken>())
            .Returns(Task.FromResult(CombinedResult<TwitchToken, CacheProviderError>.Success(cachedToken)));

        //ACT
        var res = await _service.GetAccessToken();

        //ASSERT
        Assert.That(res.IsSuccess, Is.True);
        Assert.That(res.Value, Is.EqualTo(cachedToken));
        await _cacheProvider.Received(1).Get<TwitchToken>(TwitchAuthTokenCacheKey, Arg.Any<System.Threading.CancellationToken>());
        await _cacheProvider.DidNotReceive().AcquireLock(
            Arg.Any<string>(),
            Arg.Any<TimeSpan>(),
            Arg.Any<System.Threading.CancellationToken>());
        await _innerService.Received(0).GetAccessToken();
        _logger.ReceivedLog(LogLevel.Information, "Using cached Twitch Auth token.");
    }
    
    [Test]
    public async Task GetAccessToken_ShouldCallInnerService_WhenTokenInCacheExpired()
    {
        //ARRANGE
        var now = DateTimeOffset.Now;
        _timeProvider.SetUtcNow(now);
        var past = now - TimeSpan.FromHours(1);
        
        var cachedToken = _fixture.Build<TwitchToken>().With(x=>x.ExpiresOn, past).Create();
        _cacheProvider.Get<TwitchToken>(TwitchAuthTokenCacheKey, Arg.Any<System.Threading.CancellationToken>())
            .Returns(Task.FromResult(CombinedResult<TwitchToken, CacheProviderError>.Success(cachedToken)));

        var newToken = _fixture.Create<TwitchToken>();
        _innerService.GetAccessToken().Returns(ValueResult<TwitchToken>.Success(newToken));

        //ACT
        var res = await _service.GetAccessToken();

        //ASSERT
        Assert.That(res.IsSuccess, Is.True);
        Assert.That(res.Value, Is.EqualTo(newToken));
        await _cacheProvider.Received(2).Get<TwitchToken>(TwitchAuthTokenCacheKey, Arg.Any<System.Threading.CancellationToken>());
        await _cacheProvider.Received(1).AcquireLock(
            TwitchAuthLockKey,
            TimeSpan.FromSeconds(TwitchAuthLockDurationInSeconds),
            Arg.Any<System.Threading.CancellationToken>());
        await _lockLease.Received(1).DisposeAsync();
        await _innerService.Received(1).GetAccessToken();
        await _cacheProvider.Received(1).Set(TwitchAuthTokenCacheKey, newToken, Arg.Is<CacheOptions>(x => x != null && x.AbsoluteExpiration.HasValue), Arg.Any<System.Threading.CancellationToken>());
        _logger.ReceivedLog(2, LogLevel.Information, "Cached token is expired.");
        _logger.NotReceivedLog(LogLevel.Information, "Using cached Twitch Auth token.");
        _logger.ReceivedLog(LogLevel.Information, "Caching new Twitch Auth token.");
    }

    [Test]
    public async Task GetAccessToken_WhenAnotherCallerCachesTokenWhileWaitingForLock_ReturnsCachedToken()
    {
        //ARRANGE
        var cachedToken = _fixture.Build<TwitchToken>()
            .With(x => x.ExpiresOn, _timeProvider.GetUtcNow() + TimeSpan.FromHours(1))
            .Create();
        var cacheMiss = CombinedResult<TwitchToken, CacheProviderError>.Failure(CacheProviderError.NotFound);
        var cacheHit = CombinedResult<TwitchToken, CacheProviderError>.Success(cachedToken);
        var getCallCount = 0;

        _cacheProvider.Get<TwitchToken>(TwitchAuthTokenCacheKey, Arg.Any<System.Threading.CancellationToken>())
            .Returns(_ => Task.FromResult(++getCallCount == 1 ? cacheMiss : cacheHit));

        //ACT
        var result = await _service.GetAccessToken();

        //ASSERT
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(cachedToken));
        await _cacheProvider.Received(2).Get<TwitchToken>(TwitchAuthTokenCacheKey, Arg.Any<System.Threading.CancellationToken>());
        await _cacheProvider.Received(1).AcquireLock(
            TwitchAuthLockKey,
            TimeSpan.FromSeconds(TwitchAuthLockDurationInSeconds),
            Arg.Any<System.Threading.CancellationToken>());
        await _lockLease.Received(1).DisposeAsync();
        await _innerService.DidNotReceive().GetAccessToken();
        await _cacheProvider.DidNotReceive().Set(
            Arg.Any<string>(),
            Arg.Any<TwitchToken>(),
            Arg.Any<CacheOptions>(),
            Arg.Any<System.Threading.CancellationToken>());
    }

    [Test]
    public async Task GetAccessToken_ShouldCallInnerServiceAndSavesNewTokenOnSuccess_IfNoTokenInCache_WithDefaultExpiration()
    {
        //ARRANGE
        var now = DateTimeOffset.Now;
        _timeProvider.SetUtcNow(now);
        
        _cacheProvider.Get<TwitchToken>(TwitchAuthTokenCacheKey, Arg.Any<System.Threading.CancellationToken>())
            .Returns(Task.FromResult(CombinedResult<TwitchToken, CacheProviderError>.Failure(CacheProviderError.NotFound)));

        // 1 < TwitchAuthTokenCacheDurationInHours
        var newToken = _fixture.Build<TwitchToken>().With(x=> x.ExpiresOn, now + TimeSpan.FromHours(1)).Create();
        _innerService.GetAccessToken().Returns(ValueResult<TwitchToken>.Success(newToken));

        //ACT
        var res = await _service.GetAccessToken();

        //ASSERT
        Assert.That(res.IsSuccess, Is.True);
        Assert.That(res.Value, Is.EqualTo(newToken));
        await _cacheProvider.Received(2).Get<TwitchToken>(TwitchAuthTokenCacheKey, Arg.Any<System.Threading.CancellationToken>());
        await _cacheProvider.Received(1).AcquireLock(
            TwitchAuthLockKey,
            TimeSpan.FromSeconds(TwitchAuthLockDurationInSeconds),
            Arg.Any<System.Threading.CancellationToken>());
        await _lockLease.Received(1).DisposeAsync();
        await _innerService.Received(1).GetAccessToken();
        await _cacheProvider.Received(1).Set(TwitchAuthTokenCacheKey, newToken, Arg.Is<CacheOptions>(x => x != null && x.AbsoluteExpiration.HasValue && x.AbsoluteExpiration.Value.TotalHours == TwitchAuthTokenCacheDurationInHours), Arg.Any<System.Threading.CancellationToken>());
        _logger.ReceivedLog(LogLevel.Information, "Caching new Twitch Auth token.");
    }
    
    [Test]
    public async Task GetAccessToken_ShouldCallInnerServiceAndSavesNewTokenOnSuccess_IfNoTokenInCache_WithTokenExpiration()
    {
        //ARRANGE
        var now = DateTimeOffset.Now;
        _timeProvider.SetUtcNow(now);
        
        _cacheProvider.Get<TwitchToken>(TwitchAuthTokenCacheKey, Arg.Any<System.Threading.CancellationToken>())
            .Returns(Task.FromResult(CombinedResult<TwitchToken, CacheProviderError>.Failure(CacheProviderError.NotFound)));

        // 2 * TwitchAuthTokenCacheDurationInHours > TwitchAuthTokenCacheDurationInHours
        var tokenExpiration = TwitchAuthTokenCacheDurationInHours * 2;
        var newToken = _fixture.Build<TwitchToken>().With(x=> x.ExpiresOn, now + TimeSpan.FromHours(tokenExpiration)).Create();
        _innerService.GetAccessToken().Returns(ValueResult<TwitchToken>.Success(newToken));

        //ACT
        var res = await _service.GetAccessToken();

        //ASSERT
        Assert.That(res.IsSuccess, Is.True);
        Assert.That(res.Value, Is.EqualTo(newToken));
        await _cacheProvider.Received(2).Get<TwitchToken>(TwitchAuthTokenCacheKey, Arg.Any<System.Threading.CancellationToken>());
        await _cacheProvider.Received(1).AcquireLock(
            TwitchAuthLockKey,
            TimeSpan.FromSeconds(TwitchAuthLockDurationInSeconds),
            Arg.Any<System.Threading.CancellationToken>());
        await _lockLease.Received(1).DisposeAsync();
        await _innerService.Received(1).GetAccessToken();
        await _cacheProvider.Received(1).Set(TwitchAuthTokenCacheKey, newToken, Arg.Is<CacheOptions>(x => x != null && x.AbsoluteExpiration.HasValue && x.AbsoluteExpiration.Value.TotalHours == tokenExpiration), Arg.Any<System.Threading.CancellationToken>());
        _logger.ReceivedLog(LogLevel.Information, "Caching new Twitch Auth token.");
    }

    [Test]
    public async Task GetAccessToken_ShouldCallInnerServiceButDoesNotSavesNewTokenOnFailure_IfNoTokenInCache()
    {
        //ARRANGE
        _cacheProvider.Get<TwitchToken>(TwitchAuthTokenCacheKey, Arg.Any<System.Threading.CancellationToken>())
            .Returns(Task.FromResult(CombinedResult<TwitchToken, CacheProviderError>.Failure(CacheProviderError.NotFound)));

        _innerService.GetAccessToken().Returns(ValueResult<TwitchToken>.Failure());

        //ACT
        var res = await _service.GetAccessToken();

        //ASSERT
        Assert.That(res.IsSuccess, Is.False);
        await _cacheProvider.Received(2).Get<TwitchToken>(TwitchAuthTokenCacheKey, Arg.Any<System.Threading.CancellationToken>());
        await _cacheProvider.Received(1).AcquireLock(
            TwitchAuthLockKey,
            TimeSpan.FromSeconds(TwitchAuthLockDurationInSeconds),
            Arg.Any<System.Threading.CancellationToken>());
        await _lockLease.Received(1).DisposeAsync();
        await _innerService.Received(1).GetAccessToken();
        await _cacheProvider.Received(0).Set(TwitchAuthTokenCacheKey, Arg.Any<TwitchToken>(), Arg.Any<CacheOptions>(), Arg.Any<System.Threading.CancellationToken>());
        _logger.NotReceivedLog(LogLevel.Information, "Caching new Twitch Auth token.");
    }
}