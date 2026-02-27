using System.Collections.Generic;
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

public class UserListsManagerTests
{
    private const string UserListsCacheKey = "user-lists";

    private IFixture _fixture;
    private ILogger<UserListsManager> _logger;
    private IUserLoginStateManager _userLoginStateManager;
    private ICacheProvider _cacheProvider;
    private IYglListsClient _yglListsClient;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _logger = Substitute.For<ILogger<UserListsManager>>();
        _userLoginStateManager = Substitute.For<IUserLoginStateManager>();
        _cacheProvider = Substitute.For<ICacheProvider>();
        _yglListsClient = Substitute.For<IYglListsClient>();
    }

    [Test]
    public async Task Refresh_OnYglSuccessfulCall_SetsCache()
    {
        //ARRANGE
        var token = _fixture.Create<string>();
        _userLoginStateManager.GetUserToken().Returns(token);
        var lists = _fixture.Create<List<GamesListDto>>();
        _yglListsClient.GetSelfLists(token, includeGames: true).Returns(CombinedResult<List<GamesListDto>, YglListsClientError>.Success(lists));

        var userListsManager = new UserListsManager(_logger, _userLoginStateManager, _cacheProvider, _yglListsClient);

        //ACT
        var res = await userListsManager.Refresh();

        //ASSERT
        Assert.That(res.IsSuccess, Is.True);
        Assert.That(res.Value, Is.Not.Null);
        Assert.That(res.Value, Is.EquivalentTo(lists));
        await _cacheProvider.Received(1).Set(UserListsCacheKey, lists);
        _logger.ReceivedLog(LogLevel.Information, "Saving user games lists in cache.");
    }

    [Test]
    public async Task Refresh_OnYglFailedCall_ReturnsError()
    {
        //ARRANGE
        var token = _fixture.Create<string>();
        _userLoginStateManager.GetUserToken().Returns(token);
        var error = _fixture.Create<YglListsClientError>();
        _yglListsClient.GetSelfLists(token, includeGames: true).Returns(CombinedResult<List<GamesListDto>, YglListsClientError>.Failure(error));

        var userListsManager = new UserListsManager(_logger, _userLoginStateManager, _cacheProvider, _yglListsClient);

        //ACT
        var res = await userListsManager.Refresh();

        //ASSERT
        Assert.That(res.IsSuccess, Is.False);
        await _userLoginStateManager.Received(1).GetUserToken();
        await _yglListsClient.Received(1).GetSelfLists(token, includeGames: true);
        _logger.ReceivedLog(LogLevel.Warning, "Could not refresh user games lists, due to ygl api call failure.");
    }

    [Test]
    public async Task Set_Successfully_SetsListsInCache()
    {
        //ARRANGE
        var lists = _fixture.Create<List<GamesListDto>>();

        var userListsManager = new UserListsManager(_logger, _userLoginStateManager, _cacheProvider, _yglListsClient);

        //ACT
        await userListsManager.Set(lists);

        //ASSERT
        await _cacheProvider.Received(1).Set(UserListsCacheKey, lists);
        _logger.ReceivedLog(LogLevel.Information, "Saving user games lists in cache.");
    }

    [Test]
    public async Task ReadOrRefresh_IfCacheExist_ReturnsListsFromCache()
    {
        //ARRANGE
        var lists = _fixture.Create<List<GamesListDto>>();
        _cacheProvider.Get<List<GamesListDto>>(UserListsCacheKey).Returns(CombinedResult<List<GamesListDto>, CacheProviderError>.Success(lists));
        var userListsManager = new UserListsManager(_logger, _userLoginStateManager, _cacheProvider, _yglListsClient);

        //ACT
        var res = await userListsManager.ReadOrRefresh();

        //ASSERT
        Assert.That(res.IsSuccess, Is.True);
        Assert.That(res.Value, Is.Not.Null);
        Assert.That(res.Value, Is.EquivalentTo(lists));
        await _cacheProvider.Received(1).Get<List<GamesListDto>>(UserListsCacheKey);
        _logger.ReceivedLog(LogLevel.Information, "Found user games lists in cache.");
    }

    [Test]
    public async Task ReadOrRefresh_IfCacheDoesNotExist_RefreshesLists()
    {
        //ARRANGE
        var token = _fixture.Create<string>();
        _userLoginStateManager.GetUserToken().Returns(token);
        var lists = _fixture.Create<List<GamesListDto>>();
        var error = _fixture.Create<CacheProviderError>();
        _cacheProvider.Get<List<GamesListDto>>(UserListsCacheKey).Returns(CombinedResult<List<GamesListDto>, CacheProviderError>.Failure(error));
        _yglListsClient.GetSelfLists(token, includeGames: true).Returns(CombinedResult<List<GamesListDto>, YglListsClientError>.Success(lists));

        var userListsManager = new UserListsManager(_logger, _userLoginStateManager, _cacheProvider, _yglListsClient);

        //ACT
        var res = await userListsManager.ReadOrRefresh();

        //ASSERT
        Assert.That(res.IsSuccess, Is.True);
        Assert.That(res.Value, Is.Not.Null);
        Assert.That(res.Value, Is.EquivalentTo(lists));
        await _cacheProvider.Received(1).Set(UserListsCacheKey, lists);
        await _yglListsClient.Received(1).GetSelfLists(token, includeGames: true);
        await _userLoginStateManager.Received(1).GetUserToken();
        _logger.ReceivedLog(LogLevel.Information, "User games lists in cache not found. Refreshing...");
    }
}