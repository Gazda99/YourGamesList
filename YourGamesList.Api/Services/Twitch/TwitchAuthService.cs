using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YourGamesList.Api.Services.Twitch.Model.Requests;
using YourGamesList.Api.Services.Twitch.Options;
using YourGamesList.Common;
using YourGamesList.Common.Refit;

namespace YourGamesList.Api.Services.Twitch;

public class TwitchAuthService : ITwitchAuthService
{
    private readonly ILogger<TwitchAuthService> _logger;
    private readonly ITwitchAuthApi _twitchAuthApi;
    private readonly IOptions<TwitchAuthOptions> _twitchAuthOptions;

    public TwitchAuthService(
        ILogger<TwitchAuthService> logger,
        ITwitchAuthApi twitchAuthApi,
        IOptions<TwitchAuthOptions> twitchAuthOptions
    )
    {
        _logger = logger;
        _twitchAuthApi = twitchAuthApi;
        _twitchAuthOptions = twitchAuthOptions;
    }

    public string GetClientId()
    {
        return _twitchAuthOptions.Value.ClientId;
    }

    public async Task<ValueResult<string>> GetAccessToken()
    {
        var request = new TwitchAuthRequest()
        {
            ClientId = _twitchAuthOptions.Value.ClientId,
            ClientSecret = _twitchAuthOptions.Value.ClientSecret
        };

        var callResult = await _twitchAuthApi.TryRefit(
            () => _twitchAuthApi.GetAccessToken(request.ToFormUrlEncodedContent()), _logger, "Twitch Auth Api");

        if (callResult.IsFailure)
        {
            return ValueResult<string>.Failure();
        }

        var res = callResult.Value;

        if (res.StatusCode == HttpStatusCode.OK)
        {
            var accessToken = res.Content!.AccessToken;
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogError("Obtained Twitch access token is null or empty.");
                return ValueResult<string>.Failure();
            }

            _logger.LogInformation("Successfully obtained Twitch access token from Twitch.");

            return ValueResult<string>.Success(accessToken);
        }

        _logger.LogError($"Twitch responded with '{res.StatusCode}' status code. Could not obtain Twitch access token.");
        return ValueResult<string>.Failure();
    }
}