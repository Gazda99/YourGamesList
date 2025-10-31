using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Services.Auth;
using YourGamesList.Api.Services.Auth.Filters;
using YourGamesList.Api.Services.Scraper;
using YourGamesList.Api.Services.Scraper.Model;

namespace YourGamesList.Api.Controllers;

[ApiController]
[Route("scraper")]
public class ScraperController : YourGamesListBaseController
{
    private readonly ILogger<ScraperController> _logger;
    private readonly IScraperService _scraperService;

    public ScraperController(
        ILogger<ScraperController> logger,
        IScraperService scraperService
    )
    {
        _logger = logger;
        _scraperService = scraperService;
    }

    [TypeFilter(typeof(ApiKeyAuthFilterAttribute), Arguments = [ApiKeys.ScraperApiKeyName])]
    [HttpPost("scrapeGames")]
    [ProducesResponseType(typeof(void), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(void), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> StartScrape()
    {
        var res = await _scraperService.StartScrape();
        if (res.IsSuccess)
        {
            _logger.LogInformation("Scrape started");
            return Result(StatusCodes.Status202Accepted);
        }
        else if (res.Error is ScraperError.ScrapeAlreadyInProgress)
        {
            _logger.LogInformation("Scrape already in progress");
            return Result(StatusCodes.Status409Conflict);
        }
        else
        {
            _logger.LogWarning("Could not start scrape");
            return Result(StatusCodes.Status500InternalServerError);
        }
    }


    [TypeFilter(typeof(ApiKeyAuthFilterAttribute), Arguments = [ApiKeys.ScraperApiKeyName])]
    [HttpDelete("scrapeGames")]
    [ProducesResponseType(typeof(void), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StopScrape()
    {
        var res = _scraperService.StopScrape();
        if (res)
        {
            _logger.LogInformation("Scrape will be stopped");
            return Result(StatusCodes.Status202Accepted);
        }
        else
        {
            _logger.LogWarning("Scrape not started. Nothing to stop");
            return Result(StatusCodes.Status404NotFound);
        }
    }

    [TypeFilter(typeof(ApiKeyAuthFilterAttribute), Arguments = [ApiKeys.ScraperApiKeyName])]
    [HttpGet("scrapeGames")]
    //TODO: swagger show string value of that enum instead of number
    [ProducesResponseType(typeof(ScrapeStatus), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckScrapeStatus()
    {
        var scrapeStatus = _scraperService.CheckScrapeStatus();
        return Result(StatusCodes.Status200OK, scrapeStatus.ToString());
    }
}