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

    #region Lists

    [HttpGet]
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

    [HttpGet("search")]
    [Authorize]
    [ProducesResponseType(typeof(List<GamesListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Search([FromQuery] SearchListsRequest searchListsRequest)
    {
        _logger.LogInformation(
            "Requested to search lists by owner {OwnerName} and list name {ListName}.",
            searchListsRequest.OwnerName,
            searchListsRequest.ListName);

        var gamesLists = await _listsManagerService.Search(searchListsRequest.ToParams());
        return Result(StatusCodes.Status200OK, gamesLists);
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Add(CreateListRequest createListRequest)
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

    [HttpDelete("{listId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Remove(DeleteListRequest deleteListRequest)
    {
        _logger.LogInformation("Requested to remove list with id: {GamesListId}", deleteListRequest.ListId);

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

    [HttpPatch("{listId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(UpdateListRequest updateListRequest)
    {
        _logger.LogInformation("Requested to update list with id: {GamesListId}", updateListRequest.ListId);

        var dto = updateListRequest.ToParams();
        var result = await _listsManagerService.Update(dto);

        if (result.IsFailure)
        {
            if (result.Error is ListsError.ListNotFound)
            {
                return Result(StatusCodes.Status404NotFound, new ErrorResponse
                {
                    Errors = [_domainErrorMapper.Map(result.Error)]
                });
            }
            else if (result.Error is ListsError.ListAlreadyExists or ListsError.ListCannotBeEdited)
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

        return Result(StatusCodes.Status204NoContent);
    }

    #endregion

    #region Game list entries

    [HttpPost("{listId:guid}/entries")]
    [Authorize]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddGameListEntry(AddGameListEntryRequest request)
    {
        _logger.LogInformation(
            "Requested to add IGDB game {IgdbId} to list {GamesListId}.",
            request.Body.IgdbId,
            request.ListId);

        var result = await _listsManagerService.AddGameListEntry(request.ToParams());
        if (result.IsFailure)
        {
            if (result.Error is ListsError.ListNotFound)
            {
                return Result(StatusCodes.Status404NotFound, new ErrorResponse
                {
                    Errors = [_domainErrorMapper.Map(result.Error)]
                });
            }
            else if (result.Error is ListsError.GameListEntryAlreadyExists)
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

        return Result(StatusCodes.Status200OK, result.Value);
    }

    [HttpDelete("{listId:guid}/entries/{entryId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveGameListEntry(RemoveGameListEntryRequest request)
    {
        _logger.LogInformation("Requested to remove game list entry {EntryId}.", request.EntryId);

        var result = await _listsManagerService.RemoveGameListEntry(request.ToParams());
        if (result.IsFailure)
        {
            if (result.Error is ListsError.GameListEntryNotFound)
            {
                return Result(StatusCodes.Status404NotFound, new ErrorResponse
                {
                    Errors = [_domainErrorMapper.Map(result.Error)]
                });
            }
            else
            {
                return Result(StatusCodes.Status500InternalServerError);
            }
        }

        return Result(StatusCodes.Status204NoContent);
    }

    [HttpPatch("{listId:guid}/entries/{entryId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateGameListEntry(UpdateGameListEntryRequest request)
    {
        _logger.LogInformation("Requested to update game list entry {EntryId}.", request.EntryId);

        var result = await _listsManagerService.UpdateGameListEntry(request.ToParams());
        if (result.IsFailure)
        {
            if (result.Error is ListsError.GameListEntryNotFound)
            {
                return Result(StatusCodes.Status404NotFound, new ErrorResponse
                {
                    Errors = [_domainErrorMapper.Map(result.Error)]
                });
            }
            else if (result.Error is ListsError.ListCannotBeEdited)
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

        return Result(StatusCodes.Status204NoContent);
    }

    #endregion
}