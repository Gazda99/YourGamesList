using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace YourGamesList.Api.Controllers;

public class YourGamesListBaseController : ControllerBase
{
    [NonAction]
    [ApiExplorerSettings(IgnoreApi = true)]
    protected static IActionResult Result([ActionResultStatusCode] int statusCode, [ActionResultObjectValue] object? value = null)
    {
        return new ObjectResult(value ?? "")
        {
            StatusCode = statusCode
        };
    }
}