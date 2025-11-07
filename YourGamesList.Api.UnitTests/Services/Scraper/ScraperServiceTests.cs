using System;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using YourGamesList.Api.Services.Igdb;
using YourGamesList.Api.Services.Igdb.Model;
using YourGamesList.Api.Services.Scraper;
using YourGamesList.Api.Services.Scraper.Model;
using YourGamesList.Api.Services.Scraper.Options;
using YourGamesList.Common;
using YourGamesList.TestsUtils;

namespace YourGamesList.Api.UnitTests.Services.Scraper;

public class ScraperServiceTests
{
    private IFixture _fixture;
    private ILogger<ScraperService> _logger;
    private IScraperCache _scraperCache;
    private IIgdbService _igdbService;
    private IBackgroundScraper _backgroundScraper;
    private TimeProvider _timeProvider;
    private IOptions<ScraperOptions> _options;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _logger = Substitute.For<ILogger<ScraperService>>();
        _scraperCache = Substitute.For<IScraperCache>();
        _igdbService = Substitute.For<IIgdbService>();
        _backgroundScraper = Substitute.For<IBackgroundScraper>();
        _timeProvider = Substitute.For<TimeProvider>();
        _options = Substitute.For<IOptions<ScraperOptions>>();
    }

    #region StartScrape

    [Test]
    public async Task StartScrape_SuccessfullyStartsScrape()
    {
        //ARRANGE
        var options = _fixture.Create<ScraperOptions>();
        _options.Value.Returns(options);
        var now = DateTime.Now;
        _timeProvider.GetUtcNow().Returns(now);
        _scraperCache.TryGetCacheEntry(out Arg.Any<ScrapeInfoCacheEntry?>()).Returns(x =>
        {
            x[0] = null;
            return false;
        });
        var multiQueryCount = _fixture.Create<MultiQueryCount>();
        var count = multiQueryCount.Count;
        _igdbService.CallIgdb<MultiQueryCount[]>(IgdbEndpoints.MultiQuery, Arg.Any<string>())
            .Returns(ValueResult<MultiQueryCount[]>.Success([multiQueryCount]));

        var scraperService = new ScraperService(_logger, _options, _scraperCache, _igdbService, _backgroundScraper, _timeProvider);

        //ACT
        var res = await scraperService.StartScrape();

        //ASSERT
        Assert.That(res.IsSuccess, Is.True);
        await _igdbService.Received(1).CallIgdb<MultiQueryCount[]>(IgdbEndpoints.MultiQuery, Arg.Any<string>());
        _timeProvider.Received(1).GetUtcNow();
        _scraperCache.Received(1).WriteCache(Arg.Is<ScrapeInfoCacheEntry>(x => x.ScrapeStatus == ScrapeStatus.Running && x.StartTime == now));
        await _backgroundScraper.Received(1).Scrape(
            count,
            _options.Value.BatchSize,
            _options.Value.ConcurrencyLevel,
            _options.Value.RateLimitTimeFrameInMilliseconds,
            _options.Value.MaxConcurrentCallsWithinTimeFrame
        );
        _logger.ReceivedLog(LogLevel.Information, "Getting information about games count.");
        _logger.ReceivedLog(LogLevel.Information, $"Found games count: '{count}'");
        _logger.ReceivedLog(LogLevel.Information, ["Starting new scrape with id"]);
    }

    [Test]
    public async Task StartScrape_WhenScrapeAlreadyRunning_ReturnsErrorScrapeAlreadyInProgress()
    {
        //ARRANGE
        var cacheEntry = _fixture
            .Build<ScrapeInfoCacheEntry>()
            .With(x => x.ScrapeStatus, ScrapeStatus.Running)
            .Create();

        _scraperCache.TryGetCacheEntry(out Arg.Any<ScrapeInfoCacheEntry?>()).Returns(x =>
        {
            x[0] = cacheEntry;
            return true;
        });


        var scraperService = new ScraperService(_logger, _options, _scraperCache, _igdbService, _backgroundScraper, _timeProvider);

        //ACT
        var res = await scraperService.StartScrape();

        //ASSERT
        Assert.That(res.IsSuccess, Is.False);
        Assert.That(res.Error, Is.EqualTo(ScraperError.ScrapeAlreadyInProgress));
        _logger.ReceivedLog(LogLevel.Information, "Scrape status is running. Cannot start next scrape.");
    }

    [Test]
    public async Task StartScrape_WhenGetCountIgdbCallFails_ReturnsErrorGeneral()
    {
        //ARRANGE
        _scraperCache.TryGetCacheEntry(out Arg.Any<ScrapeInfoCacheEntry?>()).Returns(x =>
        {
            x[0] = null;
            return false;
        });
        _igdbService.CallIgdb<MultiQueryCount[]>(IgdbEndpoints.MultiQuery, Arg.Any<string>()).Returns(ValueResult<MultiQueryCount[]>.Failure());

        var scraperService = new ScraperService(_logger, _options, _scraperCache, _igdbService, _backgroundScraper, _timeProvider);

        //ACT
        var res = await scraperService.StartScrape();

        //ASSERT
        Assert.That(res.IsSuccess, Is.False);
        Assert.That(res.Error, Is.EqualTo(ScraperError.General));
        await _igdbService.Received(1).CallIgdb<MultiQueryCount[]>(IgdbEndpoints.MultiQuery, Arg.Any<string>());
        _logger.ReceivedLog(LogLevel.Information, "Getting information about games count.");
        _logger.ReceivedLog(LogLevel.Information, "Due to igdb api call fail, cannot start new scrape.");
    }

    [Test]
    public async Task StartScrape_WhenGetCountIgdbCallReturnsNoData_ReturnsErrorGeneral()
    {
        //ARRANGE
        _scraperCache.TryGetCacheEntry(out Arg.Any<ScrapeInfoCacheEntry?>()).Returns(x =>
        {
            x[0] = null;
            return false;
        });
        _igdbService.CallIgdb<MultiQueryCount[]>(IgdbEndpoints.MultiQuery, Arg.Any<string>()).Returns(ValueResult<MultiQueryCount[]>.Success([]));

        var scraperService = new ScraperService(_logger, _options, _scraperCache, _igdbService, _backgroundScraper, _timeProvider);

        //ACT
        var res = await scraperService.StartScrape();

        //ASSERT
        Assert.That(res.IsSuccess, Is.False);
        Assert.That(res.Error, Is.EqualTo(ScraperError.General));
        await _igdbService.Received(1).CallIgdb<MultiQueryCount[]>(IgdbEndpoints.MultiQuery, Arg.Any<string>());
        _logger.ReceivedLog(LogLevel.Information, "Getting information about games count.");
        _logger.ReceivedLog(LogLevel.Information, "Get Igdb games count response contains no results.");
    }

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