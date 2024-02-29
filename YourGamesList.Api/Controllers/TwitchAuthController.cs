using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Filters;
using YourGamesList.Common.Services.TwitchAuth;
using YourGamesList.Common.Services.TwitchAuth.Exceptions;

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
            var twitchAuthResponse = await _twitchAuthService.ObtainAccessToken();
            _logger.LogInformation(twitchAuthResponse.AccessToken);
        }
        catch (TwitchAuthException ex)
        {
            _logger.LogError(ex, "Cannot obtain twitch auth token.");
            return StatusCode((int) HttpStatusCode.ServiceUnavailable);
        }

        return Ok();
    }
}