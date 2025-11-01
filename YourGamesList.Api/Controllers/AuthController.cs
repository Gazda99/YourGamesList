using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Attributes;
using YourGamesList.Api.Model.Requests.Auth;
using YourGamesList.Api.Model.Responses.Auth;
using YourGamesList.Api.Services.Auth;
using YourGamesList.Api.Services.Auth.Model;

namespace YourGamesList.Api.Controllers;

[ApiController]
[Route("auth")]
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
    [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status409Conflict)]
    [TypeFilter(typeof(RequestValidatorAttribute<UserRegisterRequest>), Arguments = ["userRegisterRequest"])]
    public async Task<IActionResult> Register(UserRegisterRequest userRegisterRequest)
    {
        _logger.LogInformation($"Requested to register user '{userRegisterRequest.Username}'");

        var res = await _userManagerService.RegisterUser(userRegisterRequest.Username, userRegisterRequest.Password);

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

        return Result(StatusCodes.Status200OK);
    }

    [HttpPost("login")]
    [ProducesResponseType<AuthLoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [TypeFilter(typeof(RequestValidatorAttribute<UserLoginRequest>), Arguments = ["userLoginRequest"])]
    public async Task<IActionResult> Login(UserLoginRequest userLoginRequest)
    {
        _logger.LogInformation($"Requested to login user '{userLoginRequest.Username}'");

        var res = await _userManagerService.Login(userLoginRequest.Username, userLoginRequest.Password);

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

    [HttpPost("delete")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [TypeFilter(typeof(RequestValidatorAttribute<UserDeleteRequest>), Arguments = ["userDeleteRequest"])]
    public async Task<IActionResult> Delete(UserDeleteRequest userDeleteRequest)
    {
        _logger.LogInformation($"Requested to delete user '{userDeleteRequest.Username}'");

        var res = await _userManagerService.Delete(userDeleteRequest.Username, userDeleteRequest.Password);

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