using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Attributes;
using YourGamesList.Api.Model.Requests.Auth;

namespace YourGamesList.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : YourGamesListBaseController
{
    private readonly ILogger<AuthController> _logger;

    public AuthController(ILogger<AuthController> logger)
    {
        _logger = logger;
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [TypeFilter(typeof(RequestValidatorAttribute<UserRegisterRequest>), Arguments = ["userRegisterRequest"])]
    public async Task<IActionResult> Register(UserRegisterRequest userRegisterRequest)
    {
        return Result(StatusCodes.Status200OK, userRegisterRequest.Username);
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [TypeFilter(typeof(RequestValidatorAttribute<UserLoginRequest>), Arguments = ["userLoginRequest"])]
    public async Task<IActionResult> Login(UserLoginRequest userLoginRequest)
    {
        return Result(StatusCodes.Status200OK);
    }

    [HttpPost("delete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [TypeFilter(typeof(RequestValidatorAttribute<UserDeleteRequest>), Arguments = ["userDeleteRequest"])]
    public async Task<IActionResult> Delete(UserDeleteRequest userDeleteRequest)
    {
        return Result(StatusCodes.Status200OK);
    }
}