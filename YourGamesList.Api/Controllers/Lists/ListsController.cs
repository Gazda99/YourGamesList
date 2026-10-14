using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Attributes;
using YourGamesList.Api.Model.Requests.Lists;
using YourGamesList.Api.Services;
using YourGamesList.Api.Services.Lists;
using YourGamesList.Api.Services.Lists.Model;
using YourGamesList.Contracts.Dto;
using YourGamesList.Contracts.Responses;

namespace YourGamesList.Api.Controllers.Lists;

[ApiController]
[Route("lists")]
[RequestValidator]
public class ListsController : YourGamesListBaseController
{
    private readonly ILogger<ListsController> _logger;
    private readonly IListsManagerService _listsManagerService;
    private readonly IDomainErrorMapper _domainErrorMapper;

    public ListsController(
        ILogger<ListsController> logger,
        IListsManagerService listsManagerService,
        IDomainErrorMapper domainErrorMapper
    )
    {
        _logger = logger;
        _listsManagerService = listsManagerService;
        _domainErrorMapper = domainErrorMapper;
    }

    [HttpGet("getSelf")]
    [Authorize]
    [ProducesResponseType(typeof(List<GamesListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSelf(GetSelfListRequest getSelfListRequest)
    {
        _logger.LogInformation("Requested to get self lists");

        var dto = getSelfListRequest.ToParams();

        var result = await _listsManagerService.GetSelf(dto);

        if (result.IsFailure)
        {
            if (result.Error is ListsError.UserNotFound)
            {
                return Result(StatusCodes.Status404NotFound, new ErrorResponse
                {
                    Errors = [_domainErrorMapper.Map(result.Error)]
                });
            }
            else if (result.Error is ListsError.InternalServerError)
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
            return Result(StatusCodes.Status200OK, result.Value);
        }
    }

    [HttpPost("create")]
    [Authorize]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(CreateListRequest createListRequest)
    {
        _logger.LogInformation("Requested to create list");

        var dto = createListRequest.ToParams();

        var result = await _listsManagerService.Create(dto);

        if (result.IsFailure)
        {
            if (result.Error is ListsError.ListAlreadyExists)
            {
                return Result(StatusCodes.Status409Conflict, new ErrorResponse
                {
                    Errors = [_domainErrorMapper.Map(result.Error)]
                });
            }
            else if (result.Error is ListsError.UserNotFound)
            {
                return Result(StatusCodes.Status404NotFound, new ErrorResponse
                {
                    Errors = [_domainErrorMapper.Map(result.Error)]
                });
            }
            else if (result.Error is ListsError.InternalServerError)
            {
                return Result(StatusCodes.Status400BadRequest, _domainErrorMapper.Map(result.Error));
            }
            else
            {
                return Result(StatusCodes.Status500InternalServerError);
            }
        }
        else
        {
            return Result(StatusCodes.Status200OK, result.Value);
        }
    }

    [HttpDelete("delete")]
    [Authorize]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(DeleteListRequest deleteListRequest)
    {
        _logger.LogInformation("Requested to delete list with id: {GamesListId}", deleteListRequest.Body.Id);

        var dto = deleteListRequest.ToParams();

        var result = await _listsManagerService.Delete(dto);

        if (result.IsFailure)
        {
            if (result.Error is ListsError.ListNotFound)
            {
                return Result(StatusCodes.Status404NotFound, new ErrorResponse
                {
                    Errors = [_domainErrorMapper.Map(result.Error)]
                });
            }
            else if (result.Error is ListsError.ListCannotBeDeleted)
            {
                return Result(StatusCodes.Status409Conflict, new ErrorResponse
                {
                    Errors = [_domainErrorMapper.Map(result.Error)]
                });
            }
            else if (result.Error is ListsError.InternalServerError)
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
            return Result(StatusCodes.Status204NoContent);
        }
    }
}