using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Attributes;
using YourGamesList.Api.Model.Requests.Users;
using YourGamesList.Api.OutputCachePolicies;
using YourGamesList.Api.Services;
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
    private readonly ICountriesService _countriesService;

    public UsersController(ILogger<UsersController> logger, IRequestToParametersMapper requestToParametersMapper, IUsersService usersService,
        ICountriesService countriesService)
    {
        _logger = logger;
        _requestToParametersMapper = requestToParametersMapper;
        _usersService = usersService;
        _countriesService = countriesService;
    }


    [HttpGet("getSelf")]
    [Authorize]
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
    [Authorize]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
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
        else if (res.Error == UsersError.UserUpdateWrongInputData)
        {
            return Result(StatusCodes.Status400BadRequest);
        }
        else
        {
            return Result(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("countries")]
    [Authorize]
    [OutputCache(PolicyName = nameof(AlwaysOnOkOutputPolicy))]
    [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
    public Task<IActionResult> GetAvailableCountries()
    {
        var res = _countriesService.GetAllIsoCodes();
        return Task.FromResult(Result(StatusCodes.Status200OK, res));
    }
}