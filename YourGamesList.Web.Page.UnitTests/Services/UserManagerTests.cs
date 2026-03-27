using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Logging;
using NSubstitute;
using YourGamesList.Common;
using YourGamesList.Common.Caching;
using YourGamesList.Contracts.Dto;
using YourGamesList.TestsUtils;
using YourGamesList.Web.Page.Services;
using YourGamesList.Web.Page.Services.UserLoginStateManager;
using YourGamesList.Web.Page.Services.Ygl;
using YourGamesList.Web.Page.Services.Ygl.Model;

namespace YourGamesList.Web.Page.UnitTests.Services;

public class UserManagerTests
{
    private const string UserCacheKey = "user-dto-entry";

    private IFixture _fixture;
    private ILogger<UserManager> _logger;
    private IUserLoginStateManager _userLoginStateManager;
    private ICacheProvider _cacheProvider;
    private IYglUsersClient _yglUsersClient;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _logger = Substitute.For<ILogger<UserManager>>();
        _userLoginStateManager = Substitute.For<IUserLoginStateManager>();
        _cacheProvider = Substitute.For<ICacheProvider>();
        _yglUsersClient = Substitute.For<IYglUsersClient>();
    }

    [Test]
    public async Task Refresh_OnYglSuccessfulCall_SetsCache()
    {
        //ARRANGE
        var token = _fixture.Create<string>();
        _userLoginStateManager.GetUserToken().Returns(token);
        var user = _fixture.Create<UserDto>();
        _yglUsersClient.GetSelfUser(token).Returns(CombinedResult<UserDto, YglUserClientError>.Success(user));

        var userManager = new UserManager(_logger, _userLoginStateManager, _cacheProvider, _yglUsersClient);

        //ACT
        var res = await userManager.Refresh();

        //ASSERT
        Assert.That(res.IsSuccess, Is.True);
        Assert.That(res.Value, Is.Not.Null);
        Assert.That(res.Value, Is.EqualTo(user));
        await _cacheProvider.Received(1).Set(UserCacheKey, user);
        _logger.ReceivedLog(LogLevel.Information, "Saving user in cache.");
    }

    [Test]
    public async Task Refresh_OnYglFailedCall_ReturnsError()
    {
        //ARRANGE
        var token = _fixture.Create<string>();
        _userLoginStateManager.GetUserToken().Returns(token);
        var error = _fixture.Create<YglUserClientError>();
        _yglUsersClient.GetSelfUser(token).Returns(CombinedResult<UserDto, YglUserClientError>.Failure(error));

        var userManager = new UserManager(_logger, _userLoginStateManager, _cacheProvider, _yglUsersClient);

        //ACT
        var res = await userManager.Refresh();

        //ASSERT
        Assert.That(res.IsSuccess, Is.False);
        await _userLoginStateManager.Received(1).GetUserToken();
        await _yglUsersClient.Received(1).GetSelfUser(token);
        _logger.ReceivedLog(LogLevel.Warning, "Could not refresh user, due to ygl api call failure.");
    }

    [Test]
    public async Task ReadOrRefresh_IfCacheExist_ReturnsUserFromCache()
    {
        //ARRANGE
        var user = _fixture.Create<UserDto>();
        _cacheProvider.Get<UserDto>(UserCacheKey).Returns(CombinedResult<UserDto, CacheProviderError>.Success(user));
        var userManager = new UserManager(_logger, _userLoginStateManager, _cacheProvider, _yglUsersClient);

        //ACT
        var res = await userManager.ReadOrRefresh();

        //ASSERT
        Assert.That(res.IsSuccess, Is.True);
        Assert.That(res.Value, Is.Not.Null);
        Assert.That(res.Value, Is.EqualTo(user));
        await _cacheProvider.Received(1).Get<UserDto>(UserCacheKey);
        _logger.ReceivedLog(LogLevel.Information, "Found user in cache.");
    }

    [Test]
    public async Task ReadOrRefresh_IfCacheDoesNotExist_RefreshesUser()
    {
        //ARRANGE
        var token = _fixture.Create<string>();
        _userLoginStateManager.GetUserToken().Returns(token);
        var user = _fixture.Create<UserDto>();
        var error = _fixture.Create<CacheProviderError>();
        _cacheProvider.Get<UserDto>(UserCacheKey).Returns(CombinedResult<UserDto, CacheProviderError>.Failure(error));
        _yglUsersClient.GetSelfUser(token).Returns(CombinedResult<UserDto, YglUserClientError>.Success(user));

        var userManager = new UserManager(_logger, _userLoginStateManager, _cacheProvider, _yglUsersClient);

        //ACT
        var res = await userManager.ReadOrRefresh();

        //ASSERT
        Assert.That(res.IsSuccess, Is.True);
        Assert.That(res.Value, Is.Not.Null);
        Assert.That(res.Value, Is.EqualTo(user));
        await _cacheProvider.Received(1).Set(UserCacheKey, user);
        await _yglUsersClient.Received(1).GetSelfUser(token);
        await _userLoginStateManager.Received(1).GetUserToken();
        _logger.ReceivedLog(LogLevel.Information, "User in cache not found. Refreshing...");
    }
}