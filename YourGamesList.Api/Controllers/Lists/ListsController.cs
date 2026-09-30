using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Attributes;
using YourGamesList.Api.Model.Requests.Lists;
using YourGamesList.Api.Services.Lists;
using YourGamesList.Api.Services.Lists.Model;

namespace YourGamesList.Api.Controllers.Lists;

[ApiController]
[Route("lists")]
[RequestValidator]
public class ListsController : YourGamesListBaseController
{
    private readonly ILogger<ListsController> _logger;
    private readonly IListsManagerService _listsManagerService;

    public ListsController(ILogger<ListsController> logger, IListsManagerService listsManagerService)
    {
        _logger = logger;
        _listsManagerService = listsManagerService;
    }

    [HttpPost("create")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(CreateListRequest createListRequest)
    {
        _logger.LogInformation("Requested to create list");

        var dto = createListRequest.ToDto();

        var result = await _listsManagerService.Create(dto);

        if (result.IsFailure)
        {
            if (result.Error is ListsError.InternalServerError)
            {
                return Result(StatusCodes.Status400BadRequest, result.Error.ToString());
            }
            else
            {
                return Result(StatusCodes.Status500InternalServerError);
            }
        }
        else
        {
            return Result(StatusCodes.Status200OK);
        }
    }

    [HttpDelete("delete")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(DeleteListRequest deleteListRequest)
    {
        _logger.LogInformation("Requested to delete list with id: {GamesListId}", deleteListRequest.Body.Id);

        var dto = deleteListRequest.ToDto();

        var result = await _listsManagerService.Delete(dto);

        if (result.IsFailure)
        {
            if (result.Error is ListsError.InternalServerError)
            {
                return Result(StatusCodes.Status400BadRequest, result.Error.ToString());
            }
            else
            {
                return Result(StatusCodes.Status500InternalServerError);
            }
        }
        else
        {
            return Result(StatusCodes.Status200OK);
        }
    }
}