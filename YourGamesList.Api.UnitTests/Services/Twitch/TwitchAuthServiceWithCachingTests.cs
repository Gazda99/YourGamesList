using System;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Logging;
using NSubstitute;
using YourGamesList.Api.Services.Twitch;
using YourGamesList.Common;
using YourGamesList.Common.Caching;
using YourGamesList.TestsUtils;

namespace YourGamesList.Api.UnitTests.Services.Twitch;

public class TwitchAuthServiceWithCachingTests
{
    private const string TwitchAuthTokenCacheKey = "twitch-auth-token";
    private const int TwitchAuthTokenCacheDurationInHours = 24;

    private IFixture _fixture;
    private ILogger<TwitchAuthServiceWithCaching> _logger;
    private ICacheProvider _cacheProvider;
    private ITwitchAuthService _innerService;
    private TwitchAuthServiceWithCaching _service;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _logger = Substitute.For<ILogger<TwitchAuthServiceWithCaching>>();
        _cacheProvider = Substitute.For<ICacheProvider>();
        _innerService = Substitute.For<ITwitchAuthService>();
        _service = new TwitchAuthServiceWithCaching(_logger, _cacheProvider, _innerService);
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
        var cachedToken = _fixture.Create<string>();
        _cacheProvider.TryGet(TwitchAuthTokenCacheKey, out Arg.Any<string?>()).Returns(x =>
        {
            x[1] = cachedToken;
            return true;
        });

        //ACT
        var res = await _service.GetAccessToken();

        //ASSERT
        Assert.That(res.IsSuccess, Is.True);
        Assert.That(res.Value, Is.EquivalentTo(cachedToken));
        _cacheProvider.Received(1).TryGet(TwitchAuthTokenCacheKey, out Arg.Any<string?>());
        await _innerService.Received(0).GetAccessToken();
        _logger.ReceivedLog(LogLevel.Information, "Using cached Twitch Auth token.");
    }

    [Test]
    public async Task GetAccessToken_ShouldCallInnerServiceAndSavesNewTokenOnSuccess_IfNoTokenInCache()
    {
        //ARRANGE
        _cacheProvider.TryGet(TwitchAuthTokenCacheKey, out Arg.Any<string?>()).Returns(x =>
        {
            x[1] = null;
            return false;
        });

        var newToken = _fixture.Create<string>();
        _innerService.GetAccessToken().Returns(ValueResult<string>.Success(newToken));

        //ACT
        var res = await _service.GetAccessToken();

        //ASSERT
        Assert.That(res.IsSuccess, Is.True);
        Assert.That(res.Value, Is.EquivalentTo(newToken));
        _cacheProvider.Received(1).TryGet(TwitchAuthTokenCacheKey, out Arg.Any<string?>());
        await _innerService.Received(1).GetAccessToken();
        _cacheProvider.Received(1).Set(TwitchAuthTokenCacheKey, newToken, Arg.Is<TimeSpan>(x => x.TotalHours == TwitchAuthTokenCacheDurationInHours));
        _logger.ReceivedLog(LogLevel.Information, "Caching new Twitch Auth token.");
    }

    [Test]
    public async Task GetAccessToken_ShouldCallInnerServiceButDoesNotSavesNewTokenOnFailure_IfNoTokenInCache()
    {
        //ARRANGE
        _cacheProvider.TryGet(TwitchAuthTokenCacheKey, out Arg.Any<string?>()).Returns(x =>
        {
            x[1] = null;
            return false;
        });

        _innerService.GetAccessToken().Returns(ValueResult<string>.Failure());

        //ACT
        var res = await _service.GetAccessToken();

        //ASSERT
        Assert.That(res.IsSuccess, Is.False);
        _cacheProvider.Received(1).TryGet(TwitchAuthTokenCacheKey, out Arg.Any<string?>());
        await _innerService.Received(1).GetAccessToken();
        _cacheProvider.Received(0).Set(TwitchAuthTokenCacheKey, Arg.Any<string>(), Arg.Any<TimeSpan>());
        _logger.NotReceivedLog(LogLevel.Information, "Caching new Twitch Auth token.");
    }
}