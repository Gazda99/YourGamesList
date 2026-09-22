using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Attributes;
using YourGamesList.Api.Model.Requests.Users;

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
    public async Task<IActionResult> Register(UserRegisterRequest userRegisterRequest)
    {
        _logger.LogInformation("Requested to register user {Username}", userRegisterRequest.Body.Username);

        return Result(HttpStatusCode.NotImplemented);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLoginRequest userLoginRequest)
    {
        _logger.LogInformation("Requested to login user {Username}", userLoginRequest.Body.Username);

        return Result(HttpStatusCode.NotImplemented);
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> Delete(UserDeleteRequest userDeleteRequest)
    {
        _logger.LogInformation("Requested to delete user {Username}", userDeleteRequest.Body.Username);

        return Result(HttpStatusCode.NotImplemented);
    }
}