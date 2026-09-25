using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Attributes;
using YourGamesList.Api.Model.Requests.Users;
using YourGamesList.Api.Services.Auth;
using YourGamesList.Api.Services.Auth.Model;
using YourGamesList.Contracts.Responses.Users;

namespace YourGamesList.Api.Controllers.Users;

[ApiController]
[Route("users/auth")]
[RequestValidator]
public class UserAuthController : YourGamesListBaseController
{
    private readonly ILogger<UserAuthController> _logger;
    private readonly IUserManagerService _userManagerService;

    public UserAuthController(ILogger<UserAuthController> logger, IUserManagerService userManagerService)
    {
        _logger = logger;
        _userManagerService = userManagerService;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(UserRegisterRequest userRegisterRequest)
    {
        _logger.LogInformation("Requested to register user {Username}", userRegisterRequest.Body.Username);

        var dto = userRegisterRequest.ToDto();

        var result = await _userManagerService.Register(dto);

        if (result.IsFailure)
        {
            if (result.Error == UserAuthError.RegisterNameAlreadyTaken)
            {
                return Result(StatusCodes.Status409Conflict);
            }
            else if (IsErrorBadRequest(result.Error))
            {
                return Result(StatusCodes.Status400BadRequest, result.Error.ToString());
            }
            else
            {
                return Result(StatusCodes.Status500InternalServerError);
            }
        }
        else
        {
            return Result(StatusCodes.Status200OK, result.Value.ToString());
        }

        static bool IsErrorBadRequest(UserAuthError userAuthError)
        {
            return userAuthError is UserAuthError.PasswordIsTooShort or UserAuthError.PasswordIsTooLong or UserAuthError.InvalidEmail
                or UserAuthError.InvalidUsername or UserAuthError.UsernameIsTooShort or UserAuthError.UsernameIsTooLong or UserAuthError.EmailIsTooLong;
        }
    }

    [HttpPost("login")]
    [ProducesResponseType<AuthLoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Login(UserLoginRequest userLoginRequest)
    {
        _logger.LogInformation("Requested to login user {Username}", userLoginRequest.Body.Username);

        return Result(StatusCodes.Status200OK);
    }

    [HttpDelete("delete")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(UserDeleteRequest userDeleteRequest)
    {
        _logger.LogInformation("Requested to delete user {Username}", userDeleteRequest.Body.Username);

        return Result(StatusCodes.Status200OK);
    }
}