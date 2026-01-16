using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YourGamesList.Common.Caching;
using YourGamesList.Web.Page.Services.Caching;
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

    private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly ILogger<UserLoginStateManager> _logger;
    private readonly IOptions<UserLoginStateManagerOptions> _options;
    private readonly ICacheProvider _cacheProvider;

    public UserLoginStateManager(
        ILogger<UserLoginStateManager> logger,
        IOptions<UserLoginStateManagerOptions> options,
        [FromKeyedServices(WebPageCacheProviders.LocalStorage)]
        ICacheProvider cacheProvider
    )
    {
        _logger = logger;
        _options = options;
        _cacheProvider = cacheProvider;
    }

    public event Action? OnLoginStateChanged;

    public async Task SaveUserToken(string token)
    {
        var ttl = TimeSpan.FromMinutes(_options.Value.TokenTtlInMinutes);
        await _cacheProvider.Set(UserTokenLocalStorageKey, token, ttl, _jsonSerializerOptions);
        OnLoginStateChanged?.Invoke();
    }

    public async Task<bool> IsUserLoggedIn()
    {
        var tokenRes = await _cacheProvider.Get<string>(UserTokenLocalStorageKey, _jsonSerializerOptions);
        return tokenRes.IsSuccess;
    }

    public async Task<string?> GetUserToken()
    {
        var tokenRes = await _cacheProvider.Get<string>(UserTokenLocalStorageKey, _jsonSerializerOptions);
        if (tokenRes.IsSuccess)
        {
            return tokenRes.Value;
        }
        else
        {
            if (tokenRes.Error == CacheProviderError.Expired)
            {
                await RemoveUserToken();
            }

            return null;
        }
    }

    public async Task RemoveUserToken()
    {
        await _cacheProvider.Remove(UserTokenLocalStorageKey);
        OnLoginStateChanged?.Invoke();
    }
}