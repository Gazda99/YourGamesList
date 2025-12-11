using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Attributes;
using YourGamesList.Api.Model.Requests.SearchIgdbGames;
using YourGamesList.Api.Services.Igdb;
using YourGamesList.Api.Services.Igdb.Model;
using YourGamesList.Common;

namespace YourGamesList.Api.Controllers.Games;

[ApiController]
[Route("games/igdb")]
public class SearchIgdbGamesController : YourGamesListBaseController
{
    private readonly ILogger<SearchIgdbGamesController> _logger;
    private readonly IIgdbGamesService _igdbGamesService;

    public SearchIgdbGamesController(ILogger<SearchIgdbGamesController> logger, IIgdbGamesService igdbGamesService)
    {
        _logger = logger;
        _igdbGamesService = igdbGamesService;
    }

    [HttpGet("searchGameByName")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [TypeFilter(typeof(RequestValidatorAttribute<SearchIgdbGameByNameRequest>), Arguments = ["searchIgdbGameByNameRequest"])]
    public async Task<IActionResult> SearchGameByName(SearchIgdbGameByNameRequest searchIgdbGameByNameRequest)
    {
        var gameName = searchIgdbGameByNameRequest.GameName.Trim();

        var res = await _igdbGamesService.GetGamesByName(gameName);

        return HandleGetGamesResult(res);
    }

    [HttpGet("searchGameByIds")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [TypeFilter(typeof(RequestValidatorAttribute<SearchIgdbGamesByIdsRequest>), Arguments = ["searchIgdbGamesByIdsRequest"])]
    public async Task<IActionResult> SearchGameByIds(SearchIgdbGamesByIdsRequest searchIgdbGamesByIdsRequest)
    {
        var gameIds = searchIgdbGamesByIdsRequest.GameIds;
        var res = await _igdbGamesService.GetGamesByIds(gameIds);

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