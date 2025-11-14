using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Services.Igdb;
using YourGamesList.Api.Services.Igdb.Model;
using YourGamesList.Database;
using YourGamesList.Database.Entities;

namespace YourGamesList.Api.Services.Scraper;

public interface IBackgroundScraper
{
    Task Scrape(
        int itemsToFetch,
        int batchSize,
        int concurrencyLevel,
        int timeFrameInMilliseconds,
        int maxConcurrentCallsWithinTimeFrame
    );
}

//TODO: unit tests
public class BackgroundScraper : IBackgroundScraper
{
    private readonly ILogger<BackgroundScraper> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IIgdbService _igdbService;
    private readonly IScraperCache _scraperCache;
    private readonly YglDbContext _yglDbContext;
    private readonly TimeProvider _timeProvider;

    private int _totalFetched = 0;
    private int _currentOffset = 0;
    private bool _isError = false;
    private bool _shouldCancel = false;

    public BackgroundScraper(
        ILogger<BackgroundScraper> logger,
        ILoggerFactory loggerFactory,
        IIgdbService igdbService,
        IScraperCache scraperCache,
        IDbContextFactory<YglDbContext> yglDbContext,
        TimeProvider timeProvider
    )
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
        _igdbService = igdbService;
        _scraperCache = scraperCache;
        _yglDbContext = yglDbContext.CreateDbContext();
        _timeProvider = timeProvider;
    }

    public async Task Scrape(
        int itemsToFetch,
        int batchSize,
        int concurrencyLevel,
        int timeFrameInMilliseconds,
        int maxConcurrentCallsWithinTimeFrame
    )
    {
        var igdbGamesBag = new ConcurrentBag<IgdbGame>();
        var concurrencySemaphore = new SemaphoreSlim(concurrencyLevel, concurrencyLevel);
        var rateLimiterLogger = _loggerFactory.CreateLogger<RateLimiter>();
        var rateLimiter = new RateLimiter(rateLimiterLogger, _timeProvider, timeFrameInMilliseconds, maxConcurrentCallsWithinTimeFrame);
        var tasks = new Task[concurrencyLevel];

        _logger.LogInformation($"Starting '{concurrencyLevel}' scrape workers");
        for (var i = 0; i < concurrencyLevel; i++)
        {
            tasks[i] = ScrapeWorker(i, concurrencySemaphore, rateLimiter, igdbGamesBag, itemsToFetch, batchSize);
        }

        await Task.WhenAll(tasks);

        if (_isError)
        {
            _logger.LogWarning("Error occured. BackgroundScraper is stopping.");
            UpdateCacheStatus(ScrapeStatus.Error);
            return;
        }

        if (_shouldCancel)
        {
            _logger.LogWarning("Cancelled. BackgroundScraper is stopping.");
            UpdateCacheStatus(ScrapeStatus.Cancelled);
            return;
        }

        //TODO: for new let's assume, DB is empty and there will be no duplicates/collisions
        // Updates will come later
        _logger.LogDebug("Mapping igdb game object to ygl game object...");
        var gamesToAdd = new Game[igdbGamesBag.Count];
        var gameArrayIdx = 0;
        foreach (var igdbGame in igdbGamesBag)
        {
            var game = new Game()
            {
                FirstReleaseDate = igdbGame.FirstReleaseDate,
                GameType = igdbGame.GameType.Type,
                Genres = igdbGame.Genres.Select(g => g.Name).ToList(),
                IgdbGameId = igdbGame.Id,
                ImageId = igdbGame.Cover.ImageId,
                Name = igdbGame.Name,
                StoryLine = igdbGame.StoryLine,
                Summary = igdbGame.Summary,
                Themes = igdbGame.Themes.Select(t => t.Name).ToList(),
                RatingCount = igdbGame.RatingCount
            };

            gamesToAdd[gameArrayIdx] = game;
            gameArrayIdx++;
        }

        _logger.LogInformation("Adding games to database...");
        await _yglDbContext.Games.AddRangeAsync(gamesToAdd);

        _logger.LogInformation("Saving database changes...");
        await _yglDbContext.SaveChangesAsync();
        _logger.LogInformation("Scrape process finished. Updating cache entry");

        UpdateCacheStatus(ScrapeStatus.Completed);
    }

    private async Task ScrapeWorker(
        int scrapeWorkerId,
        SemaphoreSlim concurrencySemaphore,
        RateLimiter rateLimiter,
        ConcurrentBag<IgdbGame> concurrentBag,
        int itemsToFetch,
        int batchSize
    )
    {
        using (_logger.BeginScope(new Dictionary<string, object>() { ["ScrapeWorkerId"] = scrapeWorkerId }))
        {
            var i = 0;
            while (Volatile.Read(ref _totalFetched) < itemsToFetch && !_isError && !_shouldCancel)
            {
                ++i;
                //Ensure we do not exceed allowed concurrency level
                await concurrencySemaphore.WaitAsync();

                //Ensure we do not exceed allowed request per seconds limit
                await rateLimiter.WaitAsync();

                //Calculate new offset
                var offset = Interlocked.Add(ref _currentOffset, batchSize) - batchSize;

                if (Volatile.Read(ref _totalFetched) >= itemsToFetch && !_isError && !_shouldCancel)
                {
                    _logger.LogInformation("Finishing scrape worker");
                    concurrencySemaphore.Release();
                    return;
                }

                //Every 2 calls check if cancellation was requested
                if (i % 2 == 0)
                {
                    if (_scraperCache.TryGetCacheEntry(out var cacheEntry))
                    {
                        if (cacheEntry.IsCancellationRequested)
                        {
                            concurrencySemaphore.Release();
                            _shouldCancel = true;
                            _logger.LogInformation("Cancellation requested. Stopping scrape process");
                            return;
                        }
                    }
                }

                var query = ApiCalypseQueryBuilder.Build()
                    .WithFields("cover.*,first_release_date,game_type.type,genres.name,id,name,rating_count,storyline,summary,themes.name")
                    .WithSort("id asc")
                    .WithOffset(offset)
                    .WithLimit(batchSize)
                    .CreateQuery();

                _logger.LogInformation($"Sending request to scrape games with offset '{offset}'");
                var res = await _igdbService.CallIgdb<IgdbGame[]>(IgdbEndpoints.Game, query);

                if (res.IsFailure)
                {
                    _logger.LogError("Request to get games failed. Stopping scrape process");
                    concurrencySemaphore.Release();
                    _isError = true;
                    return;
                }

                var fetchedCount = res.Value.Length;
                if (fetchedCount == 0)
                {
                    _logger.LogInformation("No games found in this batch. Stopping scrape process");
                    concurrencySemaphore.Release();
                    return;
                }

                var totalFetched = Interlocked.Add(ref _totalFetched, fetchedCount);

                concurrencySemaphore.Release();

                _logger.LogInformation($"Fetched in this batch: '{fetchedCount}'. First id: '{res.Value.First().Id}', Last id: '{res.Value.Last().Id}'");
                var percent = ((double) totalFetched / itemsToFetch) * 100;
                _logger.LogInformation($"Total fetched: '{totalFetched}'. Progress: '{percent:F2}%'");

                foreach (var game in res.Value)
                {
                    concurrentBag.Add(game);
                }
            }

            _logger.LogInformation("Finishing scrape worker");
        }
    }

    private void UpdateCacheStatus(ScrapeStatus status)
    {
        if (!_scraperCache.TryGetCacheEntry(out var cacheEntry))
        {
            //todo:
        }

        if (status is ScrapeStatus.Cancelled or ScrapeStatus.Completed or ScrapeStatus.Error)
        {
            cacheEntry!.EndTime = _timeProvider.GetUtcNow();
        }

        cacheEntry!.ScrapeStatus = status;
        _scraperCache.WriteCache(cacheEntry);
    }
}