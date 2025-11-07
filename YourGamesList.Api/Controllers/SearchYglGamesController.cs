using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Attributes;
using YourGamesList.Api.Model.Dto;
using YourGamesList.Api.Model.Requests.SearchYglGames;
using YourGamesList.Api.Services.Ygl.Games;
using YourGamesList.Api.Services.Ygl.Games.Model;

namespace YourGamesList.Api.Controllers;

//TODO: unit tests
[ApiController]
[Route("ygl/search")]
public class SearchYglGamesController : YourGamesListBaseController
{
    private readonly ILogger<SearchYglGamesController> _logger;
    private readonly IYglGamesService _yglGamesService;

    public SearchYglGamesController(ILogger<SearchYglGamesController> logger, IYglGamesService yglGamesService)
    {
        _logger = logger;
        _yglGamesService = yglGamesService;
    }

    [HttpGet("searchGames")]
    [Authorize]
    [ProducesResponseType(typeof(List<GameDto>), StatusCodes.Status200OK)]
    [TypeFilter(typeof(RequestValidatorAttribute<SearchYglGamesRequest>), Arguments = ["searchYglGamesRequest"])]
    public async Task<IActionResult> SearchGames(SearchYglGamesRequest searchYglGamesRequest)
    {
        var command = new SearchGamesParameters()
        {
            GameName = searchYglGamesRequest.Body.GameName,
            Skip = searchYglGamesRequest.Body.Skip,
            Take = searchYglGamesRequest.Body.Take,
        };

        var games = await _yglGamesService.SearchGames(command);
        return Result(StatusCodes.Status200OK, games);
    }
}