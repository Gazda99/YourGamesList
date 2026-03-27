using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using YourGamesList.Common;
using YourGamesList.Common.Refit;
using YourGamesList.Contracts.Dto;
using YourGamesList.Contracts.Requests.Games;
using YourGamesList.Contracts.Responses.Games;
using YourGamesList.Web.Page.Services.Ygl.Model;

namespace YourGamesList.Web.Page.Services.Ygl;

public interface IYglGamesClient
{
    Task<CombinedResult<List<GameDto>, YglGamesClientError>> SearchGames(
        string userToken,
        string gameName,
        IEnumerable<string>? themes = null,
        IEnumerable<string>? genres = null,
        string? gameType = null,
        (TypeOfDateDto typeOfData, int year)? releaseYearQuery = null,
        int skip = 0,
        int take = 10
    );

    Task<CombinedResult<AvailableSearchQueryArgumentsResponse, YglGamesClientError>> GetAvailableSearchParams(string userToken);
}

//TODO: unit tests
public class YglGamesClient : IYglGamesClient
{
    private readonly ILogger<YglGamesClient> _logger;
    private readonly IYglApi _yglApi;

    public YglGamesClient(ILogger<YglGamesClient> logger, IYglApi yglApi)
    {
        _logger = logger;
        _yglApi = yglApi;
    }

    public async Task<CombinedResult<List<GameDto>, YglGamesClientError>> SearchGames(
        string userToken,
        string gameName,
        IEnumerable<string>? themes = null,
        IEnumerable<string>? genres = null,
        string? gameType = null,
        (TypeOfDateDto typeOfData, int year)? releaseYearQuery = null,
        int skip = 0,
        int take = 10
    )
    {
        var request = new SearchYglGamesRequestBody()
        {
            GameName = gameName,
            Themes = themes?.ToArray(),
            Genres = genres?.ToArray(),
            GameType = gameType,
            Skip = skip,
            Take = take
        };

        if (releaseYearQuery != null && releaseYearQuery.Value.typeOfData != TypeOfDateDto.None)
        {
            request.ReleaseYearQuery = new ReleaseYearQuery()
            {
                TypeOfDate = releaseYearQuery.Value.typeOfData,
                Year = releaseYearQuery.Value.year,
            };
        }

        _logger.LogInformation($"Searching for game '{gameName}'.");

        var callResult = await _yglApi.SearchGames.TryRefit(() => _yglApi.SearchGames.SearchGames(userToken, request), _logger);
        if (callResult.IsFailure)
        {
            return CombinedResult<List<GameDto>, YglGamesClientError>.Failure(YglGamesClientError.General);
        }

        var res = callResult.Value;
        if (res.StatusCode == HttpStatusCode.OK)
        {
            return CombinedResult<List<GameDto>, YglGamesClientError>.Success(res.Content!);
        }
        else if (res.StatusCode == HttpStatusCode.NotFound)
        {
            return CombinedResult<List<GameDto>, YglGamesClientError>.Success([]);
        }

        else
        {
            return CombinedResult<List<GameDto>, YglGamesClientError>.Failure(YglGamesClientError.General);
        }
    }

    public async Task<CombinedResult<AvailableSearchQueryArgumentsResponse, YglGamesClientError>> GetAvailableSearchParams(string userToken)
    {
        var callResult = await _yglApi.SearchGames.TryRefit(() => _yglApi.SearchGames.GetAvailableSearchParams(userToken), _logger);
        if (callResult.IsFailure)
        {
            return CombinedResult<AvailableSearchQueryArgumentsResponse, YglGamesClientError>.Failure(YglGamesClientError.General);
        }

        var res = callResult.Value;
        if (res.StatusCode == HttpStatusCode.OK)
        {
            return CombinedResult<AvailableSearchQueryArgumentsResponse, YglGamesClientError>.Success(res.Content!);
        }
        else
        {
            return CombinedResult<AvailableSearchQueryArgumentsResponse, YglGamesClientError>.Failure(YglGamesClientError.General);
        }
    }
}