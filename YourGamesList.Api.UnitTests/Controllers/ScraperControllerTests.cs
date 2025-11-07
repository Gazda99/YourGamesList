using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using YourGamesList.Api.Controllers;
using YourGamesList.Api.Services.Scraper;
using YourGamesList.Api.Services.Scraper.Model;
using YourGamesList.Common;
using YourGamesList.TestsUtils;

namespace YourGamesList.Api.UnitTests.Controllers;

public class ScraperControllerTests
{
    private IFixture _fixture;
    private ILogger<ScraperController> _logger;
    private IScraperService _scraperService;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _logger = Substitute.For<ILogger<ScraperController>>();
        _scraperService = Substitute.For<IScraperService>();
    }

    #region StartScrape

    [Test]
    public async Task StartScrape_SuccessScenario()
    {
        //ARRANGE
        var id = _fixture.Create<string>();
        _scraperService.StartScrape().Returns(CombinedResult<string, ScraperError>.Success(id));
        var controller = new ScraperController(_logger, _scraperService);

        //ACT
        var res = await controller.StartScrape();

        //ASSERT
        Assert.That(res, Is.TypeOf<ObjectResult>());
        var objectResult = (ObjectResult) res;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status202Accepted));
        _logger.ReceivedLog(LogLevel.Information, "Scrape started");
        await _scraperService.Received(1).StartScrape();
    }

    [Test]
    public async Task StartScrape_OnScrapeAlreadyInProgress_ReturnsStatus409Conflict()
    {
        //ARRANGE
        _scraperService.StartScrape().Returns(CombinedResult<string, ScraperError>.Failure(ScraperError.ScrapeAlreadyInProgress));
        var controller = new ScraperController(_logger, _scraperService);

        //ACT
        var res = await controller.StartScrape();

        //ASSERT
        Assert.That(res, Is.TypeOf<ObjectResult>());
        var objectResult = (ObjectResult) res;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status409Conflict));
        _logger.ReceivedLog(LogLevel.Information, "Scrape already in progress");
        await _scraperService.Received(1).StartScrape();
    }

    [Test]
    public async Task StartScrape_OnOtherError_ReturnsStatus500InternalServerError()
    {
        //ARRANGE
        _scraperService.StartScrape().Returns(CombinedResult<string, ScraperError>.Failure(ScraperError.General));
        var controller = new ScraperController(_logger, _scraperService);

        //ACT
        var res = await controller.StartScrape();

        //ASSERT
        Assert.That(res, Is.TypeOf<ObjectResult>());
        var objectResult = (ObjectResult) res;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        _logger.ReceivedLog(LogLevel.Warning, "Could not start scrape");
        await _scraperService.Received(1).StartScrape();
    }

    #endregion

    #region StopScrape

    [Test]
    public async Task StopScrape_SuccessScenario()
    {
        //ARRANGE
        _scraperService.StopScrape().Returns(true);
        var controller = new ScraperController(_logger, _scraperService);

        //ACT
        var res = await controller.StopScrape();

        //ASSERT
        Assert.That(res, Is.TypeOf<ObjectResult>());
        var objectResult = (ObjectResult) res;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status202Accepted));
        _logger.ReceivedLog(LogLevel.Information, "Scrape will be stopped");
        _scraperService.Received(1).StopScrape();
    }

    [Test]
    public async Task StopScrape_OnFalse_ReturnsStatus404NotFound()
    {
        //ARRANGE
        _scraperService.StopScrape().Returns(false);
        var controller = new ScraperController(_logger, _scraperService);

        //ACT
        var res = await controller.StopScrape();

        //ASSERT
        Assert.That(res, Is.TypeOf<ObjectResult>());
        var objectResult = (ObjectResult) res;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
        _logger.ReceivedLog(LogLevel.Warning, "Scrape not started. Nothing to stop");
        _scraperService.Received(1).StopScrape();
    }

    #endregion

    #region CheckScrapeStatus

    [Test]
    public async Task CheckScrapeStatus_SuccessScenario()
    {
        //ARRANGE
        var scrapeStatus = _fixture.Create<ScrapeStatus>();
        _scraperService.CheckScrapeStatus().Returns(scrapeStatus);
        var controller = new ScraperController(_logger, _scraperService);

        //ACT
        var res = await controller.CheckScrapeStatus();

        //ASSERT
        Assert.That(res, Is.TypeOf<ObjectResult>());
        var objectResult = (ObjectResult) res;
        Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        Assert.That(objectResult.Value, Is.EquivalentTo(scrapeStatus.ToString()));
        _scraperService.Received(1).CheckScrapeStatus();
    }

    #endregion
}