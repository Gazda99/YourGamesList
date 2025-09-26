using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using YourGamesList.Common;
using YourGamesList.Common.Caching;

namespace YourGamesList.Api.Services.Twitch;

/// <summary>
/// Decorator for ITwitchAuthService that adds caching functionality.
/// </summary>
public class TwitchAuthServiceWithCaching : ITwitchAuthService
{
    private const string TwitchAuthTokenCacheKey = "twitch-auth-token";
    private const int TwitchAuthTokenCacheDurationInHours = 24;

    private readonly ILogger<TwitchAuthServiceWithCaching> _logger;
    private readonly ICacheProvider _cacheProvider;
    private readonly ITwitchAuthService _innerService;

    public TwitchAuthServiceWithCaching(
        ILogger<TwitchAuthServiceWithCaching> logger,
        ICacheProvider cacheProvider,
        ITwitchAuthService innerService
    )
    {
        _logger = logger;
        _cacheProvider = cacheProvider;
        _innerService = innerService;
    }

    public string GetClientId()
    {
        return _innerService.GetClientId();
    }

    public async Task<ValueResult<string>> GetAccessToken()
    {
        if (_cacheProvider.TryGet<string>(TwitchAuthTokenCacheKey, out var cachedToken) && !string.IsNullOrWhiteSpace(cachedToken))
        {
            _logger.LogInformation("Using cached Twitch Auth token.");
            return ValueResult<string>.Success(cachedToken);
        }

        var result = await _innerService.GetAccessToken();

        if (result.IsSuccess)
        {
            _logger.LogInformation("Caching new Twitch Auth token.");
            _cacheProvider.Set<string>(TwitchAuthTokenCacheKey, result.Value, TimeSpan.FromHours(TwitchAuthTokenCacheDurationInHours));
        }

        return result;
    }
}