using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Attributes;
using YourGamesList.Api.Model.Dto;
using YourGamesList.Api.Model.Requests.Lists;
using YourGamesList.Api.Services.ModelMappers;
using YourGamesList.Api.Services.Ygl.Lists;
using YourGamesList.Api.Services.Ygl.Lists.Model;

namespace YourGamesList.Api.Controllers;

[ApiController]
[Route("ygl/lists")]
public class ListsController : YourGamesListBaseController
{
    private readonly ILogger<ListsController> _logger;
    private readonly IRequestToParametersMapper _requestToParametersMapper;
    private readonly IListsService _listsService;

    public ListsController(ILogger<ListsController> logger, IRequestToParametersMapper requestToParametersMapper, IListsService listsService)
    {
        _logger = logger;
        _requestToParametersMapper = requestToParametersMapper;
        _listsService = listsService;
    }

    #region List

    [HttpPost("create")]
    [Authorize]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status409Conflict)]
    [TypeFilter(typeof(RequestValidatorAttribute<CreateListRequest>), Arguments = ["createListRequest"])]
    public async Task<IActionResult> CreateList(CreateListRequest createListRequest)
    {
        _logger.LogInformation($"Requested to create list '{createListRequest.Body.ListName}' for user '{createListRequest.UserInformation.UserId}'");

        var res = await _listsService.CreateList(createListRequest.UserInformation, createListRequest.Body.ListName, createListRequest.Body.Description);
        if (res.IsSuccess)
        {
            return Result(StatusCodes.Status200OK, res.Value);
        }

        else if (res.Error == ListsError.ListAlreadyExists)
        {
            return Result(StatusCodes.Status409Conflict);
        }
        else
        {
            return Result(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost("search")]
    [Authorize]
    [TypeFilter(typeof(RequestValidatorAttribute<SearchListsRequest>), Arguments = ["searchListsRequest"])]
    public async Task<IActionResult> SearchLists(SearchListsRequest searchListsRequest)
    {
        _logger.LogInformation($"Requested to search list name: '{searchListsRequest.Body.ListName}' user: '{searchListsRequest.Body.UserName}'");

        var parameters = _requestToParametersMapper.Map(searchListsRequest);

        var res = await _listsService.SearchLists(parameters);

        if (res.IsSuccess)
        {
            return Result(StatusCodes.Status200OK, res.Value);
        }

        else if (res.Error == ListsError.ListNotFound)
        {
            return Result(StatusCodes.Status404NotFound);
        }
        else
        {
            return Result(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("getSelf")]
    [Authorize]
    [ProducesResponseType(typeof(List<GamesListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [TypeFilter(typeof(RequestValidatorAttribute<GetSelfListsRequest>), Arguments = ["getSelfListsRequest"])]
    public async Task<IActionResult> GetSelfLists(GetSelfListsRequest getSelfListsRequest)
    {
        _logger.LogInformation($"Requested to get self list for user: '{getSelfListsRequest.UserInformation.UserId}'");

        var res = await _listsService.GetSelfLists(getSelfListsRequest.UserInformation, getSelfListsRequest.IncludeGames ?? false);
        if (res.IsSuccess)
        {
            return Result(StatusCodes.Status200OK, res.Value);
        }
        else if (res.Error == ListsError.ListNotFound)
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
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(void), StatusCodes.Status409Conflict)]
    [TypeFilter(typeof(RequestValidatorAttribute<UpdateListRequest>), Arguments = ["updateListRequest"])]
    public async Task<IActionResult> UpdateList(UpdateListRequest updateListRequest)
    {
        _logger.LogInformation($"Requested to update list '{updateListRequest.Body.ListId}' for user '{updateListRequest.UserInformation.UserId}'");

        var parameters = _requestToParametersMapper.Map(updateListRequest);
        var res = await _listsService.UpdateList(parameters);
        if (res.IsSuccess)
        {
            return Result(StatusCodes.Status200OK, res.Value);
        }
        else if (res.Error == ListsError.ListNotFound)
        {
            return Result(StatusCodes.Status404NotFound);
        }
        else if (res.Error == ListsError.ListAlreadyExists)
        {
            return Result(StatusCodes.Status409Conflict);
        }
        else
        {
            return Result(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpDelete("delete")]
    [Authorize]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(void), StatusCodes.Status423Locked)]
    [TypeFilter(typeof(RequestValidatorAttribute<DeleteListRequest>), Arguments = ["deleteListRequest"])]
    public async Task<IActionResult> DeleteList(DeleteListRequest deleteListRequest)
    {
        _logger.LogInformation($"Requested to delete list '{deleteListRequest.ListId}' for user '{deleteListRequest.UserInformation.UserId}'");

        var res = await _listsService.DeleteList(deleteListRequest.UserInformation, deleteListRequest.ListId);

        if (res.IsSuccess)
        {
            return Result(StatusCodes.Status200OK, res.Value);
        }

        if (res.Error == ListsError.ListNotFound)
        {
            return Result(StatusCodes.Status404NotFound);
        }
        else if (res.Error == ListsError.ListHardLocked)
        {
            return Result(StatusCodes.Status423Locked);
        }
        else
        {
            return Result(StatusCodes.Status500InternalServerError);
        }
    }

    #endregion

    #region ListEntry

    [HttpPost("entries/add")]
    [Authorize]
    [ProducesResponseType(typeof(List<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [TypeFilter(typeof(RequestValidatorAttribute<AddEntriesToListRequest>), Arguments = ["addEntriesToListRequest"])]
    public async Task<IActionResult> AddListEntries(AddEntriesToListRequest addEntriesToListRequest)
    {
        _logger.LogInformation($"Requested to add entries to list '{addEntriesToListRequest.Body.ListId}' for user '{addEntriesToListRequest.UserInformation.UserId}'");
        
        var parameters = _requestToParametersMapper.Map(addEntriesToListRequest);

        var res = await _listsService.AddListEntries(parameters);
        if (res.IsSuccess)
        {
            return Result(StatusCodes.Status200OK, res.Value);
        }
        else if (res.Error == ListsError.ListNotFound)
        {
            return Result(StatusCodes.Status404NotFound);
        }
        else
        {
            return Result(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost("entries/delete")]
    [Authorize]
    [ProducesResponseType(typeof(List<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [TypeFilter(typeof(RequestValidatorAttribute<DeleteListEntriesRequest>), Arguments = ["deleteListEntriesRequest"])]
    public async Task<IActionResult> DeleteListEntries(DeleteListEntriesRequest deleteListEntriesRequest)
    {
        _logger.LogInformation($"Requested to delete entries from list '{deleteListEntriesRequest.Body.ListId}' for user '{deleteListEntriesRequest.UserInformation.UserId}'");
        
        var parameters = _requestToParametersMapper.Map(deleteListEntriesRequest);

        var res = await _listsService.DeleteListEntries(parameters);
        if (res.IsSuccess)
        {
            return Result(StatusCodes.Status200OK, res.Value);
        }
        else if (res.Error == ListsError.ListNotFound)
        {
            return Result(StatusCodes.Status404NotFound);
        }
        else
        {
            return Result(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPatch("entries/update")]
    [Authorize]
    [ProducesResponseType(typeof(List<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [TypeFilter(typeof(RequestValidatorAttribute<UpdateListEntriesRequest>), Arguments = ["updateListEntriesRequest"])]
    public async Task<IActionResult> UpdateListEntries(UpdateListEntriesRequest updateListEntriesRequest)
    {
        _logger.LogInformation($"Requested to update entries in list '{updateListEntriesRequest.Body.ListId}' for user '{updateListEntriesRequest.UserInformation.UserId}'");
        
        var parameters = _requestToParametersMapper.Map(updateListEntriesRequest);

        var res = await _listsService.UpdateListEntries(parameters);
        if (res.IsSuccess)
        {
            return Result(StatusCodes.Status200OK, res.Value);
        }
        else if (res.Error == ListsError.ListNotFound)
        {
            return Result(StatusCodes.Status404NotFound);
        }
        else
        {
            return Result(StatusCodes.Status500InternalServerError);
        }
    }

    #endregion
}