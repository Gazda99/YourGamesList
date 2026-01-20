using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using YourGamesList.Common;
using YourGamesList.Common.Caching;
using YourGamesList.Web.Page.Services.UserLoginStateManager.Options;

namespace YourGamesList.Web.Page.UnitTests.Services.UserLoginStateManager;

public class UserLoginStateManagerTests
{
    private const string UserTokenLocalStorageKey = "ygl-user-token";

    private IFixture _fixture;
    private ILogger<Page.Services.UserLoginStateManager.UserLoginStateManager> _logger;
    private IOptions<UserLoginStateManagerOptions> _options;
    private ICacheProvider _cacheProvider;
    private bool _eventRaised = false;
    private Action _testEvent;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _logger = Substitute.For<ILogger<Page.Services.UserLoginStateManager.UserLoginStateManager>>();
        _cacheProvider = Substitute.For<ICacheProvider>();
        _options = Substitute.For<IOptions<UserLoginStateManagerOptions>>();
        _testEvent = () => _eventRaised = true;
    }

    [Test]
    public async Task SaveUserToken_SavesUserToken()
    {
        //ARRANGE
        var token = _fixture.Create<string>();
        var options = _fixture.Create<UserLoginStateManagerOptions>();
        _options.Value.Returns(options);
        var userLoginStateManager = new Page.Services.UserLoginStateManager.UserLoginStateManager(_logger, _options, _cacheProvider);
        userLoginStateManager.OnLoginStateChanged += _testEvent;

        //ACT
        await userLoginStateManager.SaveUserToken(token);

        //ASSERT
        await _cacheProvider.Received(1).Set(UserTokenLocalStorageKey, token, Arg.Is<TimeSpan>(t => t.TotalMinutes == options.TokenTtlInMinutes),
            Arg.Any<JsonSerializerOptions>());
        Assert.That(_eventRaised, Is.True);
    }

    [Test]
    [TestCaseSource(nameof(IsUserLoggedInTestCases))]
    public async Task IsUserLoggedIn_ReturnsCorrectBool(CombinedResult<string, CacheProviderError> cacheResult, bool expectedResult)
    {
        //ARRANGE
        var userLoginStateManager = new Page.Services.UserLoginStateManager.UserLoginStateManager(_logger, _options, _cacheProvider);
        _cacheProvider.Get<string>(UserTokenLocalStorageKey, Arg.Any<JsonSerializerOptions?>()).Returns(cacheResult);

        //ACT
        var res = await userLoginStateManager.IsUserLoggedIn();

        //ASSERT
        await _cacheProvider.Received(1).Get<string>(UserTokenLocalStorageKey, Arg.Any<JsonSerializerOptions?>());
        Assert.That(res, Is.EqualTo(expectedResult));
    }

    private static IEnumerable<TestCaseData> IsUserLoggedInTestCases()
    {
        var fixture = new Fixture();
        yield return new TestCaseData(CombinedResult<string, CacheProviderError>.Failure(fixture.Create<CacheProviderError>()), false);
        yield return new TestCaseData(CombinedResult<string, CacheProviderError>.Success(fixture.Create<string>()), true);
    }

    [Test]
    public async Task GetUserToken_WhenUserTokenStoredInCache_ReturnsToken()
    {
        //ARRANGE
        var userLoginStateManager = new Page.Services.UserLoginStateManager.UserLoginStateManager(_logger, _options, _cacheProvider);
        var token = _fixture.Create<string>();
        var cacheResult = CombinedResult<string, CacheProviderError>.Success(token);
        _cacheProvider.Get<string>(UserTokenLocalStorageKey, Arg.Any<JsonSerializerOptions?>()).Returns(cacheResult);

        //ACT
        var res = await userLoginStateManager.GetUserToken();

        //ASSERT
        await _cacheProvider.Received(1).Get<string>(UserTokenLocalStorageKey, Arg.Any<JsonSerializerOptions?>());
        Assert.That(res, Is.Not.Null);
        Assert.That(res, Is.EqualTo(token));
    }

    [Test]
    public async Task GetUserToken_WhenUserTokenNotFoundInCache_ReturnsToken()
    {
        //ARRANGE
        var userLoginStateManager = new Page.Services.UserLoginStateManager.UserLoginStateManager(_logger, _options, _cacheProvider);
        var cacheProviderError = CacheProviderError.NotFound;
        var cacheResult = CombinedResult<string, CacheProviderError>.Failure(cacheProviderError);
        _cacheProvider.Get<string>(UserTokenLocalStorageKey, Arg.Any<JsonSerializerOptions?>()).Returns(cacheResult);

        //ACT
        var res = await userLoginStateManager.GetUserToken();

        //ASSERT
        await _cacheProvider.Received(1).Get<string>(UserTokenLocalStorageKey, Arg.Any<JsonSerializerOptions?>());
        Assert.That(res, Is.Null);
    }

    [Test]
    public async Task GetUserToken_WhenUserTokenExpired_ReturnsToken()
    {
        //ARRANGE
        var userLoginStateManager = new Page.Services.UserLoginStateManager.UserLoginStateManager(_logger, _options, _cacheProvider);
        userLoginStateManager.OnLoginStateChanged += _testEvent;
        var cacheProviderError = CacheProviderError.Expired;
        var cacheResult = CombinedResult<string, CacheProviderError>.Failure(cacheProviderError);
        _cacheProvider.Get<string>(UserTokenLocalStorageKey, Arg.Any<JsonSerializerOptions?>()).Returns(cacheResult);

        //ACT
        var res = await userLoginStateManager.GetUserToken();

        //ASSERT
        await _cacheProvider.Received(1).Get<string>(UserTokenLocalStorageKey, Arg.Any<JsonSerializerOptions?>());
        await _cacheProvider.Received(1).Remove(UserTokenLocalStorageKey);
        Assert.That(_eventRaised, Is.True);
        Assert.That(res, Is.Null);
    }

    [Test]
    public async Task RemoveUserToken_RemovesUserToken()
    {
        //ARRANGE
        var userLoginStateManager = new Page.Services.UserLoginStateManager.UserLoginStateManager(_logger, _options, _cacheProvider);
        userLoginStateManager.OnLoginStateChanged += _testEvent;

        //ACT
        await userLoginStateManager.RemoveUserToken();

        //ASSERT
        await _cacheProvider.Received(1).Remove(UserTokenLocalStorageKey);
        Assert.That(_eventRaised, Is.True);
    }
}