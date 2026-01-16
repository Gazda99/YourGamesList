using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using YourGamesList.Common;
using YourGamesList.Common.Caching;
using YourGamesList.Contracts.Dto;
using YourGamesList.Web.Page.Services.UserLoginStateManager;
using YourGamesList.Web.Page.Services.Ygl;

namespace YourGamesList.Web.Page.Services;

public interface IUserListsManager
{
    Task<ValueResult<List<GamesListDto>>> Refresh();
    Task Set(List<GamesListDto> userLists);
    Task<ValueResult<List<GamesListDto>>> ReadOrRefresh();
}

public class UserListsManager : IUserListsManager
{
    private const string UserListsCacheKey = "user-lists";

    private readonly ILogger<UserListsManager> _logger;
    private readonly IUserLoginStateManager _userLoginStateManager;
    private readonly ICacheProvider _cacheProvider;
    private readonly IYglListsClient _yglListsClient;

    public UserListsManager(
        ILogger<UserListsManager> logger,
        IUserLoginStateManager userLoginStateManager,
        [FromKeyedServices(CacheProviders.InMemory)]
        ICacheProvider cacheProvider,
        IYglListsClient yglListsClient
    )
    {
        _logger = logger;
        _userLoginStateManager = userLoginStateManager;
        _cacheProvider = cacheProvider;
        _yglListsClient = yglListsClient;
    }

    public async Task<ValueResult<List<GamesListDto>>> Refresh()
    {
        var token = await _userLoginStateManager.GetUserToken();
        var userListsRes = await _yglListsClient.GetSelfLists(token!, includeGames: true);
        if (userListsRes.IsFailure)
        {
            return ValueResult<List<GamesListDto>>.Failure();
        }

        var userLists = userListsRes.Value ?? [];
        await _cacheProvider.Set(UserListsCacheKey, userLists);
        return ValueResult<List<GamesListDto>>.Success(userLists);
    }

    public async Task Set(List<GamesListDto> userLists)
    {
        await _cacheProvider.Set(UserListsCacheKey, userLists);
    }

    public async Task<ValueResult<List<GamesListDto>>> ReadOrRefresh()
    {
        var userResult = await _cacheProvider.Get<List<GamesListDto>>(UserListsCacheKey);
        if (!userResult.IsSuccess)
        {
            return await Refresh();
        }
        else
        {
            return ValueResult<List<GamesListDto>>.Success(userResult.Value);
        }
    }
}