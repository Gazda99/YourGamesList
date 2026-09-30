using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Attributes;
using YourGamesList.Api.Model.Requests.Users;
using YourGamesList.Api.Services.YourGamesList;
using YourGamesList.Api.Services.YourGamesList.Model;
using YourGamesList.Contracts.Dto;

namespace YourGamesList.Api.Controllers.Users;

[ApiController]
[Route("users")]
[RequestValidator]
public class UserController : YourGamesListBaseController
{
    private readonly ILogger<UserController> _logger;
    private readonly IUsersService _usersService;

    public UserController(ILogger<UserController> logger, IUsersService usersService)
    {
        _logger = logger;
        _usersService = usersService;
    }

    [HttpGet("getSelf")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSelfUser(UserGetSelfRequest userGetSelfRequest)
    {
        _logger.LogInformation("Requested to get self user.");

        var userGetSelfParams = userGetSelfRequest.ToParams();

        var result = await _usersService.GetSelfUser(userGetSelfParams);

        if (result.IsFailure)
        {
            if (result.Error == UsersError.UserNotFound)
            {
                Result(StatusCodes.Status404NotFound);
            }

            return Result(StatusCodes.Status500InternalServerError);
        }
        else
        {
            return Result(StatusCodes.Status200OK, result.Value);
        }
    }
}