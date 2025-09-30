using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Attributes;
using YourGamesList.Api.Model.Requests.SearchGames;
using YourGamesList.Api.Services.Igdb;
using YourGamesList.Api.Services.Igdb.Model;
using YourGamesList.Common;

namespace YourGamesList.Api.Controllers;

[ApiController]
[Route("search")]
public class SearchGamesController : YourGamesListBaseController
{
    private readonly ILogger<SearchGamesController> _logger;
    private readonly IIgdbService _igdbService;

    public SearchGamesController(ILogger<SearchGamesController> logger, IIgdbService igdbService)
    {
        _logger = logger;
        _igdbService = igdbService;
    }

    [HttpGet("searchGameByName")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [TypeFilter(typeof(RequestValidatorAttribute<SearchGameByNameRequest>), Arguments = ["searchGameByNameRequest"])]
    public async Task<IActionResult> SearchGameByName(SearchGameByNameRequest searchGameByNameRequest)
    {
        var gameName = searchGameByNameRequest.GameName.Trim();

        var res = await _igdbService.GetGamesByName(gameName);

        return HandleGetGamesResult(res);
    }

    [HttpGet("searchGameByIds")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [TypeFilter(typeof(RequestValidatorAttribute<SearchGamesByIdsRequest>), Arguments = ["searchGamesByIdsRequest"])]
    public async Task<IActionResult> SearchGameByIds(SearchGamesByIdsRequest searchGamesByIdsRequest)
    {
        var gameIds = searchGamesByIdsRequest.GameIds;
        var res = await _igdbService.GetGamesByIds(gameIds);

        return HandleGetGamesResult(res);
    }

    private IActionResult HandleGetGamesResult(ValueResult<IgdbGame[]> res)
    {
        if (res.IsSuccess)
        {
            var n = res.Value.Length;

            if (res.Value.Length == 0)
            {
                _logger.LogInformation("No games found, returning 404.");
                return Result(StatusCodes.Status404NotFound);
            }

            _logger.LogInformation($"Successfully obtained '{n}' games. Returning 200.");
            return Result(StatusCodes.Status200OK, res.Value);
        }
        else
        {
            _logger.LogError("Unhandled error. Returning 500.");
            return Result(StatusCodes.Status500InternalServerError);
        }
    }
}