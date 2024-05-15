using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Filters;
using YourGamesList.Services.Twitch.Exceptions;
using YourGamesList.Services.Twitch.Services;

namespace YourGamesList.Api.Controllers;

[ApiController]
[TypeFilter(typeof(YglExceptionFilterAttribute))]
[Route("twitchAuth")]
public class TwitchAuthController : YglControllerBase
{
    private readonly ILogger<TwitchAuthController> _logger;
    private readonly ITwitchAuthService _twitchAuthService;

    public TwitchAuthController(ILogger<TwitchAuthController> logger, ITwitchAuthService twitchAuthService)
    {
        _logger = logger;
        _twitchAuthService = twitchAuthService;
    }

    [HttpPost("obtainAccessToken")]
    public async Task<IActionResult> ObtainAccessToken()
    {
        try
        {
            var twitchAuthToken = await _twitchAuthService.ObtainAccessToken();
            _logger.LogDebug($"Access token: {twitchAuthToken}");
            
            return Ok(twitchAuthToken);
        }
        catch (TwitchAuthException ex)
        {
            _logger.LogError(ex, "Cannot obtain twitch auth token.");
            return StatusCode((int) HttpStatusCode.ServiceUnavailable);
        }

    }
}