﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Attributes;
using YourGamesList.Api.Model.Requests.SearchIgdbGames;
using YourGamesList.Api.Services.Igdb;
using YourGamesList.Api.Services.Igdb.Model;
using YourGamesList.Common;

namespace YourGamesList.Api.Controllers;

//TODO: unit tests
[ApiController]
[Route("igdb/search")]
public class SearchIgdbGamesController : YourGamesListBaseController
{
    private readonly ILogger<SearchIgdbGamesController> _logger;
    private readonly IGamesIgdbService _gamesIgdbService;

    public SearchIgdbGamesController(ILogger<SearchIgdbGamesController> logger, IGamesIgdbService gamesIgdbService)
    {
        _logger = logger;
        _gamesIgdbService = gamesIgdbService;
    }

    [HttpGet("searchGameByName")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [TypeFilter(typeof(RequestValidatorAttribute<SearchIgdbGameByNameRequest>), Arguments = ["searchIgdbGameByNameRequest"])]
    public async Task<IActionResult> SearchGameByName(SearchIgdbGameByNameRequest searchIgdbGameByNameRequest)
    {
        var gameName = searchIgdbGameByNameRequest.GameName.Trim();

        var res = await _gamesIgdbService.GetGamesByName(gameName);

        return HandleGetGamesResult(res);
    }

    [HttpGet("searchGameByIds")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [TypeFilter(typeof(RequestValidatorAttribute<SearchIgdbGamesByIdsRequest>), Arguments = ["searchIgdbGamesByIdsRequest"])]
    public async Task<IActionResult> SearchGameByIds(SearchIgdbGamesByIdsRequest searchIgdbGamesByIdsRequest)
    {
        var gameIds = searchIgdbGamesByIdsRequest.GameIds;
        var res = await _gamesIgdbService.GetGamesByIds(gameIds);

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