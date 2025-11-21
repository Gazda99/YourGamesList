using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using YourGamesList.Common;
using YourGamesList.Common.Refit;
using YourGamesList.Contracts.Dto;
using YourGamesList.Web.Page.Services.Ygl.Model;

namespace YourGamesList.Web.Page.Services.Ygl;

public interface IYglListsClient
{
    Task<CombinedResult<List<GamesListDto>, YglListsClientError>> GetSelfLists(string userToken, bool includeGames = false);
}

//TODO: unit tests
public class YglListsClient : IYglListsClient
{
    private readonly ILogger<YglListsClient> _logger;
    private readonly IYglApi _yglApi;

    public YglListsClient(ILogger<YglListsClient> logger, IYglApi yglApi)
    {
        _logger = logger;
        _yglApi = yglApi;
    }

    public async Task<CombinedResult<List<GamesListDto>, YglListsClientError>> GetSelfLists(string userToken, bool includeGames = false)
    {
        var callResult = await _yglApi.TryRefit(() => _yglApi.GetSelfLists(userToken, includeGames), _logger);
        if (callResult.IsFailure)
        {
            return CombinedResult<List<GamesListDto>, YglListsClientError>.Failure(YglListsClientError.General);
        }

        var res = callResult.Value;
        if (res.StatusCode == HttpStatusCode.OK)
        {
            return CombinedResult<List<GamesListDto>, YglListsClientError>.Success(res.Content!);
        }
        else
        {
            return CombinedResult<List<GamesListDto>, YglListsClientError>.Failure(YglListsClientError.General);
        }
    }
}