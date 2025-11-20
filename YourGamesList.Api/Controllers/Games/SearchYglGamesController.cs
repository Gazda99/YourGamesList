using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Attributes;
using YourGamesList.Api.Model.Requests.SearchYglGames;
using YourGamesList.Api.OutputCachePolicies;
using YourGamesList.Api.Services.Ygl.Games;
using YourGamesList.Api.Services.Ygl.Games.Model;
using YourGamesList.Contracts.Dto;
using YourGamesList.Contracts.Responses.Games;

namespace YourGamesList.Api.Controllers.Games;

[ApiController]
[Route("games/ygl")]
public class SearchYglGamesController : YourGamesListBaseController
{
    private readonly ILogger<SearchYglGamesController> _logger;
    private readonly IYglGamesService _yglGamesService;
    private readonly IAvailableSearchQueryArgumentsService _availableSearchQueryArgumentsService;

    public SearchYglGamesController(
        ILogger<SearchYglGamesController> logger,
        IYglGamesService yglGamesService,
        IAvailableSearchQueryArgumentsService availableSearchQueryArgumentsService
    )
    {
        _logger = logger;
        _yglGamesService = yglGamesService;
        _availableSearchQueryArgumentsService = availableSearchQueryArgumentsService;
    }

    [HttpGet("paramsForSearching")]
    [Authorize]
    [OutputCache(PolicyName = nameof(AlwaysOnOkOutputPolicy))]
    [ProducesResponseType(typeof(AvailableSearchQueryArgumentsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailableSearchParams()
    {
        var res = await _availableSearchQueryArgumentsService.GetAvailableSearchParams();
        return Result(StatusCodes.Status200OK, res);
    }

    [HttpGet("search")]
    [Authorize]
    [ProducesResponseType(typeof(List<GameDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [TypeFilter(typeof(RequestValidatorAttribute<SearchYglGamesRequest>), Arguments = ["searchYglGamesRequest"])]
    public async Task<IActionResult> SearchGames(SearchYglGamesRequest searchYglGamesRequest)
    {
        var command = new SearchGamesParameters()
        {
            GameName = searchYglGamesRequest.Body.GameName,
            Themes = searchYglGamesRequest.Body.Themes,
            Genres = searchYglGamesRequest.Body.Genres,
            GameType = searchYglGamesRequest.Body.GameType,
            Skip = searchYglGamesRequest.Body.Skip,
            Take = searchYglGamesRequest.Body.Take,
        };

        if (searchYglGamesRequest.Body.ReleaseYearQuery != null)
        {
            command.Year = searchYglGamesRequest.Body.ReleaseYearQuery.Year;
            command.TypeOfDate = searchYglGamesRequest.Body.ReleaseYearQuery.TypeOfDate;
        }

        var games = await _yglGamesService.SearchGames(command);
        if (!games.Any())
        {
            return Result(StatusCodes.Status404NotFound);
        }
        else
        {
            return Result(StatusCodes.Status200OK, games);
        }
    }
}