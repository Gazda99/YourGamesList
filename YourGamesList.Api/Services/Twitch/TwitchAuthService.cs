using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YourGamesList.Api.Services.Twitch.Model;
using YourGamesList.Api.Services.Twitch.Model.Requests;
using YourGamesList.Api.Services.Twitch.Options;
using YourGamesList.Common;
using YourGamesList.Common.Refit;

namespace YourGamesList.Api.Services.Twitch;

public class TwitchAuthService : ITwitchAuthService
{
    private const int ExpirationThresholdInSeconds = 15 * 60;

    private readonly ILogger<TwitchAuthService> _logger;
    private readonly ITwitchAuthApi _twitchAuthApi;
    private readonly IOptions<TwitchAuthOptions> _twitchAuthOptions;
    private readonly TimeProvider _timeProvider;

    public TwitchAuthService(
        ILogger<TwitchAuthService> logger,
        ITwitchAuthApi twitchAuthApi,
        IOptions<TwitchAuthOptions> twitchAuthOptions,
        TimeProvider timeProvider
    )
    {
        _logger = logger;
        _twitchAuthApi = twitchAuthApi;
        _twitchAuthOptions = twitchAuthOptions;
        _timeProvider = timeProvider;
    }

    public string GetClientId()
    {
        return _twitchAuthOptions.Value.ClientId;
    }

    public async Task<ValueResult<TwitchToken>> GetAccessToken()
    {
        var request = new TwitchAuthRequest()
        {
            ClientId = _twitchAuthOptions.Value.ClientId,
            ClientSecret = _twitchAuthOptions.Value.ClientSecret
        };

        var callResult = await _twitchAuthApi.TryRefit(() => _twitchAuthApi.GetAccessToken(request.ToFormUrlEncodedContent()), _logger, "Twitch Auth Api");

        if (callResult.IsFailure)
        {
            return ValueResult<TwitchToken>.Failure();
        }

        var res = callResult.Value;

        if (res.StatusCode == HttpStatusCode.OK)
        {
            var content = res.Content;

            if (content == null || string.IsNullOrEmpty(content?.AccessToken))
            {
                _logger.LogError("Obtained Twitch access token is null or empty.");
                return ValueResult<TwitchToken>.Failure();
            }

            _logger.LogInformation("Successfully obtained Twitch access token from Twitch that will expire in {TwitchTokenExpiryInSeconds}.", content.ExpiresIn);
            var now = _timeProvider.GetUtcNow();
            var finalExpirySeconds = content.ExpiresIn - ExpirationThresholdInSeconds;
            _logger.LogInformation("Calculated token expiration is {TwitchTokenFinalExpiryInSeconds}.", finalExpirySeconds);
            
            if (finalExpirySeconds <= 0)
            {
                _logger.LogError("Invalid final expiration in seconds for Twitch access token.");
                return ValueResult<TwitchToken>.Failure();
            }

            var expiresOn = now + TimeSpan.FromSeconds(content.ExpiresIn) - TimeSpan.FromSeconds(ExpirationThresholdInSeconds);

            var twitchToken = new TwitchToken()
            {
                AccessToken = content.AccessToken,
                ExpiresOn = expiresOn
            };

            return ValueResult<TwitchToken>.Success(twitchToken);
        }

        _logger.LogError("Twitch responded with {StatusCode} status code. Could not obtain Twitch access token.", res.StatusCode);
        return ValueResult<TwitchToken>.Failure();
    }
}