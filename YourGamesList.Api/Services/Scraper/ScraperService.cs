using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YourGamesList.Api.Services.Igdb;
using YourGamesList.Api.Services.Igdb.Model;
using YourGamesList.Api.Services.Scraper.Model;
using YourGamesList.Api.Services.Scraper.Options;
using YourGamesList.Common;

namespace YourGamesList.Api.Services.Scraper;

public interface IScraperService
{
    Task<CombinedResult<string, ScraperError>> StartScrape();
    bool StopScrape();
    ScrapeStatus CheckScrapeStatus();
}

//TODO: unit tests
public class ScraperService : IScraperService
{
    private readonly ILogger<ScraperService> _logger;
    private readonly IScraperCache _scraperCache;
    private readonly IIgdbService _igdbService;
    private readonly IBackgroundScraper _backgroundScraper;
    private readonly TimeProvider _timeProvider;
    private readonly IOptions<ScraperOptions> _options;

    public ScraperService(
        ILogger<ScraperService> logger,
        IOptions<ScraperOptions> options,
        IScraperCache scraperCache,
        IIgdbService igdbService,
        IBackgroundScraper backgroundScraper,
        TimeProvider timeProvider
    )
    {
        _logger = logger;
        _options = options;
        _scraperCache = scraperCache;
        _igdbService = igdbService;
        _backgroundScraper = backgroundScraper;
        _timeProvider = timeProvider;
    }

    public async Task<CombinedResult<string, ScraperError>> StartScrape()
    {
        if (_scraperCache.TryGetCacheEntry(out var cacheEntry))
        {
            if (cacheEntry.ScrapeStatus is ScrapeStatus.Running)
            {
                _logger.LogInformation("Scrape status is running. Cannot start next scrape.");
                return CombinedResult<string, ScraperError>.Failure(ScraperError.ScrapeAlreadyInProgress);
            }
        }

        var queryCount = ApiCalypseQueryBuilder.Build()
            .WithCustomQuery("games/count \"Count of games\" {}")
            .CreateQuery();

        _logger.LogInformation("Getting information about games count.");

        var getCountResult = await _igdbService.CallIgdb<MultiQueryCount[]>(IgdbEndpoints.MultiQuery, queryCount);

        if (getCountResult.IsFailure)
        {
            _logger.LogInformation("Due to igdb api call fail, cannot start new scrape.");
            return CombinedResult<string, ScraperError>.Failure(ScraperError.General);
        }

        if (getCountResult.Value.Length == 0)
        {
            _logger.LogInformation("Get Igdb games count response contains no results.");
            return CombinedResult<string, ScraperError>.Failure(ScraperError.General);
        }

        var count = getCountResult.Value[0].Count;

        _logger.LogInformation($"Found games count: '{count}'");

        var id = Guid.NewGuid().ToString("N");

        _logger.LogInformation($"Starting new scrape with id '{id}'");

        var now = _timeProvider.GetUtcNow();

        var newCacheEntry = new ScrapeInfoCacheEntry()
        {
            Id = id,
            StartTime = now,
            ScrapeStatus = ScrapeStatus.Running
        };
        _scraperCache.WriteCache(newCacheEntry);

        Task.Run(async () => await _backgroundScraper.Scrape(
           // count,
            1500,
            _options.Value.BatchSize,
            _options.Value.ConcurrencyLevel,
            _options.Value.RateLimitTimeFrameInMilliseconds,
            _options.Value.MaxConcurrentCallsWithinTimeFrame
        ));

        return CombinedResult<string, ScraperError>.Success(id);
    }

    public bool StopScrape()
    {
        if (!_scraperCache.TryGetCacheEntry(out var cacheEntry))
        {
            _logger.LogInformation("No scrape status found. Nothing to stop.");
            return false;
        }

        cacheEntry.IsCancellationRequested = true;
        _scraperCache.WriteCache(cacheEntry);

        return true;
    }

    public ScrapeStatus CheckScrapeStatus()
    {
        if (!_scraperCache.TryGetCacheEntry(out var cacheEntry))
        {
            _logger.LogInformation("No scrape status found.");
            return ScrapeStatus.NotRunning;
        }

        _logger.LogInformation($"Scrape status: '{cacheEntry.ScrapeStatus}' for '{cacheEntry.Id}'");
        return cacheEntry.ScrapeStatus;
    }
}