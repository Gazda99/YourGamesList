using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Attributes;
using YourGamesList.Api.Model.Requests.Auth;
using YourGamesList.Api.Services.Auth;
using YourGamesList.Api.Services.Auth.Model;
using YourGamesList.Contracts.Responses.Users;

namespace YourGamesList.Api.Controllers.Users;

[ApiController]
[Route("users/auth")]
public class AuthController : YourGamesListBaseController
{
    private readonly ILogger<AuthController> _logger;
    private readonly IUserManagerService _userManagerService;

    public AuthController(
        ILogger<AuthController> logger,
        IUserManagerService userManagerService
    )
    {
        _logger = logger;
        _userManagerService = userManagerService;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status409Conflict)]
    [TypeFilter(typeof(RequestValidatorAttribute<UserRegisterRequest>), Arguments = ["userRegisterRequest"])]
    public async Task<IActionResult> Register(UserRegisterRequest userRegisterRequest)
    {
        _logger.LogInformation($"Requested to register user '{userRegisterRequest.Body.Username}'");

        var res = await _userManagerService.RegisterUser(userRegisterRequest.Body.Username, userRegisterRequest.Body.Password);

        if (res.IsFailure)
        {
            if (res.Error == UserAuthError.RegisterNameAlreadyTaken)
            {
                return Result(StatusCodes.Status409Conflict);
            }

            if (res.Error is UserAuthError.PasswordIsTooShort or UserAuthError.PasswordIsTooLong)
            {
                return Result(StatusCodes.Status400BadRequest, res.Error.ToString());
            }
            else
            {
                return Result(StatusCodes.Status500InternalServerError);
            }
        }

        return Result(StatusCodes.Status200OK, res.Value.ToString());
    }

    [HttpPost("login")]
    [ProducesResponseType<AuthLoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [TypeFilter(typeof(RequestValidatorAttribute<UserLoginRequest>), Arguments = ["userLoginRequest"])]
    public async Task<IActionResult> Login(UserLoginRequest userLoginRequest)
    {
        _logger.LogInformation($"Requested to login user '{userLoginRequest.Body.Username}'");

        var res = await _userManagerService.Login(userLoginRequest.Body.Username, userLoginRequest.Body.Password);

        if (res.IsFailure)
        {
            if (res.Error == UserAuthError.NoUserFound)
            {
                return Result(StatusCodes.Status404NotFound);
            }
            else if (res.Error == UserAuthError.WrongPassword)
            {
                return Result(StatusCodes.Status401Unauthorized);
            }
            else
            {
                return Result(StatusCodes.Status500InternalServerError);
            }
        }

        var response = new AuthLoginResponse()
        {
            Token = res.Value
        };

        return Result(StatusCodes.Status200OK, response);
    }

    [HttpDelete("delete")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [TypeFilter(typeof(RequestValidatorAttribute<UserDeleteRequest>), Arguments = ["userDeleteRequest"])]
    public async Task<IActionResult> Delete(UserDeleteRequest userDeleteRequest)
    {
        _logger.LogInformation($"Requested to delete user '{userDeleteRequest.Body.Username}'");

        var res = await _userManagerService.Delete(userDeleteRequest.Body.Username, userDeleteRequest.Body.Password);

        if (res.IsFailure)
        {
            if (res.Error == UserAuthError.NoUserFound)
            {
                return Result(StatusCodes.Status404NotFound);
            }
            else if (res.Error == UserAuthError.WrongPassword)
            {
                return Result(StatusCodes.Status401Unauthorized);
            }
            else
            {
                return Result(StatusCodes.Status500InternalServerError);
            }
        }

        return Result(StatusCodes.Status204NoContent);
    }
}