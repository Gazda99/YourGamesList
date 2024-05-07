using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Filters;
using YourGamesList.Services.Hltb.Model;
using YourGamesList.Services.Hltb.Services;

namespace YourGamesList.Api.Controllers;

[ApiController]
[TypeFilter(typeof(YglExceptionFilterAttribute))]
[Route("hltb")]
public class HltbController : YglControllerBase
{
    private readonly ILogger<HltbController> _logger;
    private readonly IHltbService _hltbService;

    public HltbController(ILogger<HltbController> logger, IHltbService hltbService)
    {
        _logger = logger;
        _hltbService = hltbService;
    }

    [HttpGet("getHltbData/{gameName}")]
    public async Task<IActionResult> GetHowLongToBeatDataForGame(string gameName)
    {
        var hltbResponse = await _hltbService.GetHowLongToBeatDataForGame(gameName);
        return Ok(new HltbSearchResponseSimple(hltbResponse!.Data[0]));
    }
}