using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Services.Twitch;

namespace YourGamesList.Api.Controllers;

[ApiController]
[Route("test")]
public class TestController : YourGamesListBaseController
{
    private readonly ILogger<TestController> _logger;
    private readonly ITwitchAuthService _twitchAuthService;

    public TestController(
        ILogger<TestController> logger,
        ITwitchAuthService twitchAuthService
    )
    {
        _logger = logger;
        _twitchAuthService = twitchAuthService;
    }

    [HttpPost("/test")]
    public async Task<IActionResult> Test()
    {
        var res = await _twitchAuthService.GetAccessToken();
        if (res.IsSuccess)
        {
            return Result(StatusCodes.Status200OK, res.Value);
        }
        else
        {
            return Result(StatusCodes.Status500InternalServerError);
        }
    }
}