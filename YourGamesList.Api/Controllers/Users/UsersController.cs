using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Attributes;
using YourGamesList.Api.Model.Requests.Users;
using YourGamesList.Api.Services.ModelMappers;
using YourGamesList.Api.Services.Users;
using YourGamesList.Api.Services.Users.Model;
using YourGamesList.Contracts.Dto;

namespace YourGamesList.Api.Controllers.Users;

[ApiController]
[Route("users")]
public class UsersController : YourGamesListBaseController
{
    private readonly ILogger<UsersController> _logger;
    private readonly IRequestToParametersMapper _requestToParametersMapper;
    private readonly IUsersService _usersService;

    public UsersController(ILogger<UsersController> logger, IRequestToParametersMapper requestToParametersMapper, IUsersService usersService)
    {
        _logger = logger;
        _requestToParametersMapper = requestToParametersMapper;
        _usersService = usersService;
    }

    [HttpGet("getSelf")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [TypeFilter(typeof(RequestValidatorAttribute<UserGetSelfRequest>), Arguments = ["userGetSelfRequest"])]
    public async Task<IActionResult> GetSelf(UserGetSelfRequest userGetSelfRequest)
    {
        _logger.LogInformation($"Requested to get self user '{userGetSelfRequest.UserInformation.UserId}'");

        var parameters = _requestToParametersMapper.Map(userGetSelfRequest);

        var res = await _usersService.GetSelfUser(parameters);
        if (res.IsSuccess)
        {
            return Result(StatusCodes.Status200OK, res.Value);
        }
        else if (res.Error == UsersError.UserNotFound)
        {
            return Result(StatusCodes.Status404NotFound);
        }
        else
        {
            return Result(StatusCodes.Status500InternalServerError);
        }
    }


    [HttpPatch("update")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [TypeFilter(typeof(RequestValidatorAttribute<UserUpdateRequest>), Arguments = ["userUpdateRequest"])]
    public async Task<IActionResult> UpdateUser(UserUpdateRequest userUpdateRequest)
    {
        _logger.LogInformation($"Requested to update user '{userUpdateRequest.UserInformation.UserId}'");

        var parameters = _requestToParametersMapper.Map(userUpdateRequest);

        var res = await _usersService.UpdateUser(parameters);
        if (res.IsSuccess)
        {
            return Result(StatusCodes.Status200OK, res.Value);
        }
        else if (res.Error == UsersError.UserNotFound)
        {
            return Result(StatusCodes.Status404NotFound);
        }
        else
        {
            return Result(StatusCodes.Status500InternalServerError);
        }
    }
}