using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Attributes;
using YourGamesList.Api.Model.Requests.Lists;
using YourGamesList.Api.Services.Ygl;

namespace YourGamesList.Api.Controllers;

[ApiController]
[Route("lists")]
public class ListsController : YourGamesListBaseController
{
    private readonly ILogger<ListsController> _logger;
    private readonly IListsService _listsService;

    public ListsController(ILogger<ListsController> logger, IListsService listsService)
    {
        _logger = logger;
        _listsService = listsService;
    }

    [HttpPost("create")]
    [Authorize]
    [TypeFilter(typeof(RequestValidatorAttribute<CreateListRequest>), Arguments = ["createListRequest"])]
    public async Task<IActionResult> CreateList(CreateListRequest createListRequest)
    {
        return Result(StatusCodes.Status200OK);
    }

    [HttpPost("get")]
    [Authorize]
    [TypeFilter(typeof(RequestValidatorAttribute<GetListsRequest>), Arguments = ["getListsRequest"])]
    public async Task<IActionResult> GetLists(GetListsRequest getListsRequest)
    {
        return Result(StatusCodes.Status200OK);
    }

    [HttpPost("update")]
    [Authorize]
    [TypeFilter(typeof(RequestValidatorAttribute<UpdateListRequest>), Arguments = ["updateListRequest"])]
    public async Task<IActionResult> UpdateList(UpdateListRequest updateListRequest)
    {
        return Result(StatusCodes.Status200OK);
    }

    [HttpPost("delete")]
    [Authorize]
    [TypeFilter(typeof(RequestValidatorAttribute<DeleteListRequest>), Arguments = ["deleteListRequest"])]
    public async Task<IActionResult> DeleteList(DeleteListRequest deleteListRequest)
    {
        return Result(StatusCodes.Status200OK);
    }
}