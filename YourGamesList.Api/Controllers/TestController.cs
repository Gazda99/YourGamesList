using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace YourGamesList.Api.Controllers;

[ApiController]
[Route("test")]
public class TestController : YourGamesListBaseController
{
    private readonly ILogger<TestController> _logger;

    public TestController(ILogger<TestController> logger)
    {
        _logger = logger;
    }

    [HttpGet( "get")]
    public async Task<IActionResult> Get()
    {
        _logger.LogInformation("Hello from TestController");
        return Result(StatusCodes.Status200OK);
    }
}