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
using YourGamesList.Api.Services.ModelMapper;
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
        var res = await _listsService.CreateList(createListRequest.UserInformation, createListRequest.Body!.ListName, createListRequest.Body.Description);
        if (res.IsSuccess)
        {
            return Result(StatusCodes.Status200OK, res.Value.ToString());
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

    [HttpPost("getSelf")]
    [Authorize]
    [ProducesResponseType(typeof(List<GamesListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [TypeFilter(typeof(RequestValidatorAttribute<GetSelfListsRequest>), Arguments = ["getSelfListsRequest"])]
    public async Task<IActionResult> GetSelfLists(GetSelfListsRequest getSelfListsRequest)
    {
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

    [HttpPost("update")]
    [Authorize]
    [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(void), StatusCodes.Status409Conflict)]
    [TypeFilter(typeof(RequestValidatorAttribute<UpdateListsRequest>), Arguments = ["updateListsRequest"])]
    public async Task<IActionResult> UpdateList(UpdateListsRequest updateListsRequest)
    {
        var parameters = _requestToParametersMapper.Map(updateListsRequest);
        var res = await _listsService.UpdateLists(parameters);
        if (res.IsSuccess)
        {
            return Result(StatusCodes.Status200OK);
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
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(void), StatusCodes.Status423Locked)]
    [TypeFilter(typeof(RequestValidatorAttribute<DeleteListRequest>), Arguments = ["deleteListRequest"])]
    public async Task<IActionResult> DeleteList(DeleteListRequest deleteListRequest)
    {
        var res = await _listsService.DeleteList(deleteListRequest.UserInformation, deleteListRequest.Id);

        if (res.IsSuccess)
        {
            return Result(StatusCodes.Status204NoContent);
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
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [TypeFilter(typeof(RequestValidatorAttribute<AddEntriesToListRequest>), Arguments = ["addEntriesToListRequest"])]
    public async Task<IActionResult> AddListEntries(AddEntriesToListRequest addEntriesToListRequest)
    {
        var parameters = _requestToParametersMapper.Map(addEntriesToListRequest);

        var res = await _listsService.AddEntriesToList(parameters);
        if (res.IsSuccess)
        {
            return Result(StatusCodes.Status200OK, res.Value);
        }
        else
        {
            return Result(StatusCodes.Status500InternalServerError);
        }
    }

    #endregion
}