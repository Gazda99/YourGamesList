using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Attributes;
using YourGamesList.Api.Model.Requests.SearchGames;
using YourGamesList.Api.Services.Igdb;

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

    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [TypeFilter(typeof(RequestValidatorAttribute<SearchGamesRequest>), Arguments = ["searchGamesRequest"])]
    public async Task<IActionResult> Login(SearchGamesRequest searchGamesRequest)
    {
        var gameName = searchGamesRequest.GameName.Trim();

        var res = await _igdbService.GetGamesByName(gameName);

        if (res.IsSuccess)
        {
            if (res.Value.Length == 0)
            {
                _logger.LogInformation("No games found, returning 404.");
                return Result(StatusCodes.Status404NotFound);
            }

            _logger.LogInformation("Successfully obtained games. Returning 200.");
            return Result(StatusCodes.Status200OK, res.Value);
        }
        else
        {
            _logger.LogError("Unhandled error. Returning 500.");
            return Result(StatusCodes.Status500InternalServerError);
        }
    }
}