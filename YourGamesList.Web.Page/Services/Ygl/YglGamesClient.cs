using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using YourGamesList.Common;
using YourGamesList.Common.Refit;
using YourGamesList.Web.Page.Services.Ygl.Model;
using YourGamesList.Web.Page.Services.Ygl.Model.Requests;
using YourGamesList.Web.Page.Services.Ygl.Model.Responses;

namespace YourGamesList.Web.Page.Services.Ygl;

public interface IYglGamesClient
{
    Task<CombinedResult<List<Game>, YglGamesClientError>> SearchGames(string userToken, string gameName);
}

public class YglGamesClient : IYglGamesClient
{
    private readonly ILogger<YglGamesClient> _logger;
    private readonly IYglApi _yglApi;

    public YglGamesClient(ILogger<YglGamesClient> logger, IYglApi yglApi)
    {
        _logger = logger;
        _yglApi = yglApi;
    }

    public async Task<CombinedResult<List<Game>, YglGamesClientError>> SearchGames(string userToken, string gameName)
    {
        //TODO: pagination
        var request = new SearchGamesRequest()
        {
            GameName = gameName,
            Skip = 0,
            Take = 10
        };

        _logger.LogInformation($"Searching for game '{gameName}'.");
        var callResult = await _yglApi.TryRefit(() => _yglApi.SearchGames(userToken, request), _logger);

        if (callResult.IsFailure)
        {
            return CombinedResult<List<Game>, YglGamesClientError>.Failure(YglGamesClientError.General);
        }

        var res = callResult.Value;
        if (res.StatusCode == HttpStatusCode.OK)
        {
            return CombinedResult<List<Game>, YglGamesClientError>.Success(res.Content!);
        }
        else
        {
            return CombinedResult<List<Game>, YglGamesClientError>.Failure(YglGamesClientError.General);
        }
    }
}