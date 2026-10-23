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
    private const string TwitchAuthLockKey = "YGL_twitch-auth-lock";
    private const int TwitchAuthLockDurationInSeconds = 60;

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
        var cachedToken = await GetCachedToken();
        if (cachedToken is not null)
        {
            return ValueResult<TwitchToken>.Success(cachedToken);
        }

        await using var lease = await _cacheProvider.AcquireLock(TwitchAuthLockKey, TimeSpan.FromSeconds(TwitchAuthLockDurationInSeconds));

        // Double check cache, because we were waiting for lock for indefinitive amount of time, cache might expired in the meantime 
        cachedToken = await GetCachedToken();
        if (cachedToken is not null)
        {
            return ValueResult<TwitchToken>.Success(cachedToken);
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
            
            var cacheOptions = new CacheOptions { AbsoluteExpiration = absoluteExpiration };
            await _cacheProvider.Set(TwitchAuthTokenCacheKey, result.Value, cacheOptions);
        }

        return result;
    }

    private async Task<TwitchToken?> GetCachedToken()
    {
        var cacheResult = await _cacheProvider.Get<TwitchToken>(TwitchAuthTokenCacheKey);
        if (!cacheResult.IsSuccess || string.IsNullOrWhiteSpace(cacheResult.Value.AccessToken))
        {
            return null;
        }

        var token = cacheResult.Value;
        if (_timeProvider.GetUtcNow() >= token.ExpiresOn)
        {
            _logger.LogInformation("Cached token is expired.");
            return null;
        }

        _logger.LogInformation("Using cached Twitch Auth token.");
        return token;
    }
}