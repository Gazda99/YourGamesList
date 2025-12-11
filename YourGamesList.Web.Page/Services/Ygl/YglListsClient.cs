using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using YourGamesList.Common;
using YourGamesList.Common.Refit;
using YourGamesList.Contracts.Dto;
using YourGamesList.Contracts.Requests.Lists;
using YourGamesList.Web.Page.Services.Ygl.Model;

namespace YourGamesList.Web.Page.Services.Ygl;

public interface IYglListsClient
{
    Task<CombinedResult<List<GamesListDto>, YglListsClientError>> GetSelfLists(string userToken, bool includeGames = false);

    Task<CombinedResult<List<Guid>, YglListsClientError>> AddListEntries(
        string userToken,
        Guid listId,
        IEnumerable<long> gamesToAddIds
    );
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

    public async Task<CombinedResult<List<Guid>, YglListsClientError>> AddListEntries(
        string userToken,
        Guid listId,
        IEnumerable<long> gamesToAddIds
    )
    {
        var request = new AddEntriesToListRequestBody()
        {
            ListId = listId,
            EntriesToAdd = gamesToAddIds.Select(x => new EntryToAddRequestPart()
            {
                GameId = x
            }).ToArray()
        };

        var callResult = await _yglApi.TryRefit(() => _yglApi.AddListEntries(userToken, request), _logger);
        if (callResult.IsFailure)
        {
            return CombinedResult<List<Guid>, YglListsClientError>.Failure(YglListsClientError.General);
        }

        var res = callResult.Value;
        if (res.StatusCode == HttpStatusCode.OK)
        {
            return CombinedResult<List<Guid>, YglListsClientError>.Success(res.Content!);
        }
        else
        {
            return CombinedResult<List<Guid>, YglListsClientError>.Failure(YglListsClientError.General);
        }
    }
}