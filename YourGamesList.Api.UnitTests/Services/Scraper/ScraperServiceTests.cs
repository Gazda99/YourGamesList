using AutoFixture;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using YourGamesList.Api.Services.Igdb;
using YourGamesList.Api.Services.Scraper;
using YourGamesList.Api.Services.Scraper.Model;
using YourGamesList.Api.Services.Scraper.Options;
using YourGamesList.TestsUtils;

namespace YourGamesList.Api.UnitTests.Services.Scraper;

public class ScraperServiceTests
{
    private IFixture _fixture;
    private ILogger<ScraperService> _logger;
    private IScraperCache _scraperCache;
    private IIgdbService _igdbService;
    private IBackgroundScraper _backgroundScraper;
    private FakeTimeProvider _timeProvider;
    private IOptions<ScraperOptions> _options;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _logger = Substitute.For<ILogger<ScraperService>>();
        _scraperCache = Substitute.For<IScraperCache>();
        _igdbService = Substitute.For<IIgdbService>();
        _backgroundScraper = Substitute.For<IBackgroundScraper>();
        _timeProvider = new FakeTimeProvider();
        _options = Substitute.For<IOptions<ScraperOptions>>();
    }

    #region StartScrape

    //TODO: unit tests
    
    #endregion

    #region StopScrape

    [Test]
    public void StopScrape_CacheFound_ReturnsTrueAndStopsScraping()
    {
        //ARRANGE
        var cacheEntry = _fixture.Create<ScrapeInfoCacheEntry>();
        _scraperCache.TryGetCacheEntry(out Arg.Any<ScrapeInfoCacheEntry?>()).Returns(x =>
        {
            x[0] = cacheEntry;
            return true;
        });

        var scraperService = new ScraperService(_logger, _options, _scraperCache, _igdbService, _backgroundScraper, _timeProvider);

        //ACT
        var res = scraperService.StopScrape();

        //ASSERT
        Assert.That(res, Is.EqualTo(true));
        _scraperCache.Received(1).WriteCache(Arg.Is<ScrapeInfoCacheEntry>(x => x.IsCancellationRequested == true));
        _logger.ReceivedLog(LogLevel.Information, "Scrape will be stopped.");
    }

    [Test]
    public void StopScrape_CacheNotFound_ReturnsFalse()
    {
        //ARRANGE
        _scraperCache.TryGetCacheEntry(out Arg.Any<ScrapeInfoCacheEntry?>()).Returns(x =>
        {
            x[0] = null;
            return false;
        });

        var scraperService = new ScraperService(_logger, _options, _scraperCache, _igdbService, _backgroundScraper, _timeProvider);

        //ACT
        var res = scraperService.StopScrape();

        //ASSERT
        Assert.That(res, Is.EqualTo(false));
        _logger.ReceivedLog(LogLevel.Information, "No scrape status found. Nothing to stop.");
    }

    #endregion


    #region CheckScrapeStatus

    [Test]
    public void CheckScrapeStatus_CacheFound_ReturnsCurrentStatus()
    {
        //ARRANGE
        var cacheEntry = _fixture.Create<ScrapeInfoCacheEntry>();
        _scraperCache.TryGetCacheEntry(out Arg.Any<ScrapeInfoCacheEntry?>()).Returns(x =>
        {
            x[0] = cacheEntry;
            return true;
        });

        var scraperService = new ScraperService(_logger, _options, _scraperCache, _igdbService, _backgroundScraper, _timeProvider);

        //ACT
        var res = scraperService.CheckScrapeStatus();

        //ASSERT
        Assert.That(res, Is.EqualTo(cacheEntry.ScrapeStatus));
        _logger.ReceivedLog(LogLevel.Information, $"Scrape status: '{cacheEntry.ScrapeStatus}' for '{cacheEntry.Id}'");
    }

    [Test]
    public void CheckScrapeStatus_CacheNotFound_ReturnsNotRunning()
    {
        //ARRANGE
        _scraperCache.TryGetCacheEntry(out Arg.Any<ScrapeInfoCacheEntry?>()).Returns(x =>
        {
            x[0] = null;
            return false;
        });

        var scraperService = new ScraperService(_logger, _options, _scraperCache, _igdbService, _backgroundScraper, _timeProvider);

        //ACT
        var res = scraperService.CheckScrapeStatus();

        //ASSERT
        Assert.That(res, Is.EqualTo(ScrapeStatus.NotRunning));
        _logger.ReceivedLog(LogLevel.Information, "No scrape status found.");
    }

    #endregion
}