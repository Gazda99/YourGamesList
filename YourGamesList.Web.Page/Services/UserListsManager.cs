using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using YourGamesList.Common;
using YourGamesList.Contracts.Dto;
using YourGamesList.Web.Page.Services.StaticStorage;
using YourGamesList.Web.Page.Services.UserLoginStateManager;
using YourGamesList.Web.Page.Services.Ygl;

namespace YourGamesList.Web.Page.Services;

public interface IUserListsManager
{
    Task<ValueResult<List<GamesListDto>>> Refresh();
    Task<ValueResult<List<GamesListDto>>> ReadOrRefresh();
}

public class UserListsManager : IUserListsManager
{
    private readonly ILogger<UserListsManager> _logger;
    private readonly IUserLoginStateManager _userLoginStateManager;
    private readonly IStaticState<List<GamesListDto>> _userListsState;
    private readonly IYglListsClient _yglListsClient;

    public UserListsManager(
        ILogger<UserListsManager> logger,
        IUserLoginStateManager userLoginStateManager,
        IStaticState<List<GamesListDto>> userListsState,
        IYglListsClient yglListsClient)
    {
        _logger = logger;
        _userLoginStateManager = userLoginStateManager;
        _userListsState = userListsState;
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
        _userListsState.SetState(userLists);
        return ValueResult<List<GamesListDto>>.Success(userLists);
    }

    public async Task<ValueResult<List<GamesListDto>>> ReadOrRefresh()
    {
        var state = _userListsState.GetState();
        if (state == null)
        {
            return await Refresh();
        }
        else
        {
            return ValueResult<List<GamesListDto>>.Success(state);
        }
    }
}