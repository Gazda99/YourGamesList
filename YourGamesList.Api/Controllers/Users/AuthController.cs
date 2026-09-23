using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Attributes;
using YourGamesList.Api.Model.Requests.Users;
using YourGamesList.Contracts.Responses.Users;

namespace YourGamesList.Api.Controllers.Users;

[ApiController]
[Route("users/auth")]
[RequestValidator]
public class AuthController : YourGamesListBaseController
{
    private readonly ILogger<AuthController> _logger;

    public AuthController(ILogger<AuthController> logger)
    {
        _logger = logger;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(UserRegisterRequest userRegisterRequest)
    {
        _logger.LogInformation("Requested to register user {Username}", userRegisterRequest.Body.Username);

        return Result(StatusCodes.Status501NotImplemented);
    }

    [HttpPost("login")]
    [ProducesResponseType<AuthLoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Login(UserLoginRequest userLoginRequest)
    {
        _logger.LogInformation("Requested to login user {Username}", userLoginRequest.Body.Username);

        return Result(StatusCodes.Status501NotImplemented);
    }

    [HttpDelete("delete")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(UserDeleteRequest userDeleteRequest)
    {
        _logger.LogInformation("Requested to delete user {Username}", userDeleteRequest.Body.Username);

        return Result(StatusCodes.Status501NotImplemented);
    }
}