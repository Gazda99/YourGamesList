using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using YourGamesList.Common;
using YourGamesList.Common.Caching;
using YourGamesList.Contracts.Dto;
using YourGamesList.Web.Page.Services.UserLoginStateManager;
using YourGamesList.Web.Page.Services.Ygl;

namespace YourGamesList.Web.Page.Services;

public interface IUserManager
{
    Task<ValueResult<UserDto>> Refresh();
    Task<ValueResult<UserDto>> ReadOrRefresh();
}

public class UserManager : IUserManager
{
    private const string UserCacheKey = "user-dto-entry";

    private readonly ILogger<UserListsManager> _logger;
    private readonly IUserLoginStateManager _userLoginStateManager;
    private readonly ICacheProvider _cacheProvider;
    private readonly IYglUsersClient _yglUsersClient;

    public UserManager(ILogger<UserListsManager> logger,
        IUserLoginStateManager userLoginStateManager,
        [FromKeyedServices(CacheProviders.InMemory)]
        ICacheProvider cacheProvider,
        IYglUsersClient yglUsersClient)
    {
        _logger = logger;
        _userLoginStateManager = userLoginStateManager;
        _cacheProvider = cacheProvider;
        _yglUsersClient = yglUsersClient;
    }

    public async Task<ValueResult<UserDto>> Refresh()
    {
        var token = await _userLoginStateManager.GetUserToken();

        var userRes = await _yglUsersClient.GetSelfUser(token!);
        if (userRes.IsFailure)
        {
            return ValueResult<UserDto>.Failure();
        }

        var user = userRes.Value;
        await _cacheProvider.Set(UserCacheKey, user);

        return ValueResult<UserDto>.Success(user);
    }

    public async Task<ValueResult<UserDto>> ReadOrRefresh()
    {
        var userResult = await _cacheProvider.Get<UserDto>(UserCacheKey);
        if (!userResult.IsSuccess)
        {
            return await Refresh();
        }
        else
        {
            return ValueResult<UserDto>.Success(userResult.Value);
        }
    }
}