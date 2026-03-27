using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Services.Twitch;
using YourGamesList.Common;
using YourGamesList.Common.Refit;

namespace YourGamesList.Api.Services.Igdb;

public interface IIgdbService
{
    Task<ValueResult<TResponseFormat>> CallIgdb<TResponseFormat>(IgdbEndpoint endpoint, string query) where TResponseFormat : class;
}

public class IgdbService : IIgdbService
{
    private readonly ILogger<IgdbService> _logger;
    private readonly IIgdbApi _igdbApi;
    private readonly ITwitchAuthService _twitchAuthService;

    public IgdbService(ILogger<IgdbService> logger, IIgdbApi igdbApi, ITwitchAuthService twitchAuthService)
    {
        _logger = logger;
        _igdbApi = igdbApi;
        _twitchAuthService = twitchAuthService;
    }

    public async Task<ValueResult<TResponseFormat>> CallIgdb<TResponseFormat>(IgdbEndpoint endpoint, string query) where TResponseFormat : class
    {
        var clientId = _twitchAuthService.GetClientId();
        var accessToken = await _twitchAuthService.GetAccessToken();
        if (accessToken.IsFailure)
        {
            _logger.LogError("Getting Twitch access token failed");
            return ValueResult<TResponseFormat>.Failure();
        }

        _logger.LogInformation($"Calling IGDB API Endpoint: '{endpoint.Endpoint}'");

        var callResult = await _igdbApi.TryRefit(
            () => _igdbApi.Endpoint<TResponseFormat>(endpoint.Endpoint, accessToken.Value, clientId, query),
            _logger,
            $"IGDB API '{endpoint.Endpoint}'"
        );

        if (callResult.IsFailure)
        {
            return ValueResult<TResponseFormat>.Failure();
        }

        var res = callResult.Value;
        if (res.StatusCode == HttpStatusCode.OK)
        {
            _logger.LogInformation("Successfully obtained response from IGDB API.");
            return ValueResult<TResponseFormat>.Success(res.Content!);
        }

        _logger.LogError($"Unhandled HTTP Status Code '{(int) res.StatusCode}' from IGDB API.");
        return ValueResult<TResponseFormat>.Failure();
    }
}