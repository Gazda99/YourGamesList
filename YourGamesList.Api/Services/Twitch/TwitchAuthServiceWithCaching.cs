using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Services.Twitch.Model;
using YourGamesList.Common;
using YourGamesList.Common.Caching;

namespace YourGamesList.Api.Services.Twitch;

/// <summary>
/// Decorator for ITwitchAuthService that adds caching functionality.
/// </summary>
public class TwitchAuthServiceWithCaching : ITwitchAuthService
{
    private const string TwitchAuthTokenCacheKey = "YGL_twitch-auth-token";
    private const int TwitchAuthTokenDefaultCacheDurationInHours = 48;

    private readonly ILogger<TwitchAuthServiceWithCaching> _logger;
    private readonly ICacheProvider _cacheProvider;
    private readonly ITwitchAuthService _innerService;
    private readonly TimeProvider _timeProvider;

    public TwitchAuthServiceWithCaching(
        ILogger<TwitchAuthServiceWithCaching> logger,
        ICacheProvider cacheProvider,
        ITwitchAuthService innerService,
        TimeProvider timeProvider
    )
    {
        _logger = logger;
        _cacheProvider = cacheProvider;
        _innerService = innerService;
        _timeProvider = timeProvider;
    }

    public string GetClientId()
    {
        return _innerService.GetClientId();
    }

    public async Task<ValueResult<TwitchToken>> GetAccessToken()
    {
        var cacheResult = await _cacheProvider.Get<TwitchToken>(TwitchAuthTokenCacheKey);
        if (cacheResult.IsSuccess && !string.IsNullOrWhiteSpace(cacheResult.Value.AccessToken))
        {
            var twitchToken = cacheResult.Value;
            var now = _timeProvider.GetUtcNow();
            if (now < twitchToken.ExpiresOn)
            {
                _logger.LogInformation("Using cached Twitch Auth token.");
                return ValueResult<TwitchToken>.Success(cacheResult.Value);
            }
            else
            {
                _logger.LogInformation("Cached token is expired.");
            }
        }

        _logger.LogInformation("Not using cached Twitch Auth token. Trying to obtain new one.");
        var result = await _innerService.GetAccessToken();

        if (result.IsSuccess)
        {
            _logger.LogInformation("Caching new Twitch Auth token.");

            var now = _timeProvider.GetUtcNow();
            var defaultTtl = TimeSpan.FromHours(TwitchAuthTokenDefaultCacheDurationInHours);
            var diff = result.Value.ExpiresOn - now;
            
            var absoluteExpiration = defaultTtl > diff ? defaultTtl : diff;
            
            await _cacheProvider.Set(TwitchAuthTokenCacheKey, result.Value, new CacheOptions()
            {
                AbsoluteExpiration = absoluteExpiration
            });
        }

        return result;
    }
}