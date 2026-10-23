using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Attributes;
using YourGamesList.Api.Model.Requests.Users;
using YourGamesList.Api.Services;
using YourGamesList.Api.Services.Auth;
using YourGamesList.Api.Services.Auth.Model;
using YourGamesList.Contracts.Responses;
using YourGamesList.Contracts.Responses.Users;

namespace YourGamesList.Api.Controllers.Users;

[ApiController]
[Route("users/auth")]
[RequestValidator]
public class UserAuthController : YourGamesListBaseController
{
    private readonly ILogger<UserAuthController> _logger;
    private readonly IUserManagerService _userManagerService;
    private readonly IDomainErrorMapper _domainErrorMapper;

    public UserAuthController(
        ILogger<UserAuthController> logger,
        IUserManagerService userManagerService,
        IDomainErrorMapper domainErrorMapper
    )
    {
        _logger = logger;
        _userManagerService = userManagerService;
        _domainErrorMapper = domainErrorMapper;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(UserRegisterRequest userRegisterRequest)
    {
        _logger.LogInformation("Requested to register user {Username}", userRegisterRequest.Body.Username);

        var userRegisterParams = userRegisterRequest.ToParams();

        var result = await _userManagerService.Register(userRegisterParams);

        if (result.IsFailure)
        {
            if (result.Error is UserAuthError.UsernameAlreadyTaken or UserAuthError.EmailAlreadyTaken)
            {
                return Result(StatusCodes.Status409Conflict, new ErrorResponse
                {
                    Errors = [_domainErrorMapper.Map(result.Error)]
                });
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
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthLoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Login(UserLoginRequest userLoginRequest)
    {
        _logger.LogInformation("Requested to login user {Username}", userLoginRequest.Body.Username);

        var userLoginParams = userLoginRequest.ToParams();

        var result = await _userManagerService.Login(userLoginParams);

        if (result.IsFailure)
        {
            if (result.Error is UserAuthError.NoUserFound)
            {
                return Result(StatusCodes.Status404NotFound);
            }
            else if (result.Error is UserAuthError.UserNotActiveEmailActivation)
            {
                return Result(StatusCodes.Status403Forbidden, new ErrorResponse
                {
                    Errors = [_domainErrorMapper.Map(result.Error)]
                });
            }
            else if (result.Error is UserAuthError.UnauthorizedUser)
            {
                return Result(StatusCodes.Status401Unauthorized);
            }
            else
            {
                return Result(StatusCodes.Status500InternalServerError);
            }
        }
        else
        {
            var response = new AuthLoginResponse()
            {
                Token = result.Value
            };
            return Result(StatusCodes.Status200OK, response);
        }
    }

    [HttpDelete("remove")]
    [Authorize]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove(UserDeleteRequest userDeleteRequest)
    {
        _logger.LogInformation("Requested to remove user {Username}", userDeleteRequest.Body.Username);

        var userDeleteParams = userDeleteRequest.ToParams();

        var result = await _userManagerService.Delete(userDeleteParams);

        if (result.IsFailure)
        {
            if (result.Error is UserAuthError.NoUserFound)
            {
                return Result(StatusCodes.Status404NotFound);
            }
            else if (result.Error is UserAuthError.UnauthorizedUser)
            {
                return Result(StatusCodes.Status401Unauthorized);
            }
            else if (result.Error is UserAuthError.ForbiddenUser)
            {
                return Result(StatusCodes.Status403Forbidden, new ErrorResponse
                {
                    Errors = [_domainErrorMapper.Map(result.Error)]
                });
            }
            else
            {
                return Result(StatusCodes.Status500InternalServerError);
            }
        }
        else
        {
            return Result(StatusCodes.Status204NoContent);
        }
    }

    [HttpGet("confirmEmail")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmEmail(ConfirmEmailRequest confirmEmailRequest)
    {
        _logger.LogInformation("Requested to confirm email");

        var confirmEmailParams = confirmEmailRequest.ToParams();

        var result = await _userManagerService.ConfirmEmail(confirmEmailParams);

        if (result.IsFailure)
        {
            if (result.Error is UserAuthError.EmailVerificationInvalid or UserAuthError.EmailVerificationExpired)
            {
                return Result(StatusCodes.Status400BadRequest, new ErrorResponse
                {
                    Errors = [_domainErrorMapper.Map(result.Error)]
                });
            }
            else
            {
                return Result(StatusCodes.Status500InternalServerError);
            }
        }
        else
        {
            return Result(StatusCodes.Status200OK);
        }
    }
}