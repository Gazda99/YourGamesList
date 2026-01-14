using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YourGamesList.Web.Page.Services.LocalStorage;
using YourGamesList.Web.Page.Services.LocalStorage.Model;
using YourGamesList.Web.Page.Services.UserLoginStateManager.Options;

namespace YourGamesList.Web.Page.Services.UserLoginStateManager;

public interface IUserLoginStateManager
{
    event Action OnLoginStateChanged;
    Task SaveUserToken(string token);
    Task<bool> IsUserLoggedIn();
    Task<string?> GetUserToken();
    Task RemoveUserToken();
}

//TODO: unit tests
public class UserLoginStateManager : IUserLoginStateManager
{
    private const string UserTokenLocalStorageKey = "ygl-user-token";

    private readonly ILogger<UserLoginStateManager> _logger;
    private readonly IOptions<UserLoginStateManagerOptions> _options;
    private readonly ILocalStorageService _localStorageService;

    public UserLoginStateManager(
        ILogger<UserLoginStateManager> logger,
        IOptions<UserLoginStateManagerOptions> options,
        ILocalStorageService localStorageService
    )
    {
        _logger = logger;
        _options = options;
        _localStorageService = localStorageService;
    }

    public event Action? OnLoginStateChanged;

    public async Task SaveUserToken(string token)
    {
        var ttl = TimeSpan.FromMinutes(_options.Value.TokenTtlInMinutes);
        await _localStorageService.SetItem(UserTokenLocalStorageKey, token, ttl);
        OnLoginStateChanged?.Invoke();
    }

    public async Task<bool> IsUserLoggedIn()
    {
        var tokenRes = await _localStorageService.GetItem<string>(UserTokenLocalStorageKey);
        return tokenRes.IsSuccess;
    }

    public async Task<string?> GetUserToken()
    {
        var tokenRes = await _localStorageService.GetItem<string>(UserTokenLocalStorageKey);
        if (tokenRes.IsSuccess)
        {
            return tokenRes.Value;
        }
        else
        {
            if (tokenRes.Error == LocalStorageError.Expired)
            {
                await RemoveUserToken();
            }

            return null;
        }
    }

    public async Task RemoveUserToken()
    {
        await _localStorageService.RemoveItem(UserTokenLocalStorageKey);
        OnLoginStateChanged?.Invoke();
    }
}