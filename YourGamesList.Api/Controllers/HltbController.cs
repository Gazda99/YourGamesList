using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Filters;
using YourGamesList.Api.Log;
using YourGamesList.Common.Services.Hltb;
using YourGamesList.Common.Services.Hltb.Model;

namespace YourGamesList.Api.Controllers;

[ApiController]
[TypeFilter(typeof(InputsFilterAttribute))]
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
        using var l1 = _logger.WithCorrelationId(InputArguments.CorrelationId);
        var hltbResponse = await _hltbService.GetHowLongToBeatDataForGame(gameName);
        return Ok(new HltbSimple(hltbResponse!.Data[0]));
    }
}