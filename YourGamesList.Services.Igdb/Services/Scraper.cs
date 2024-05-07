using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YourGamesList.Common.Log;
using YourGamesList.Services.Igdb.Exceptions;
using YourGamesList.Services.Igdb.Model;
using YourGamesList.Services.Igdb.Options;
using YourGamesList.Services.Twitch.Services;

namespace YourGamesList.Services.Igdb.Services;

public class Scraper : IScraper
{
    private readonly ILogger<Scraper> _logger;
    private readonly IIgdbClient _igdbClient;
    private readonly IMaxIdChecker _maxIdChecker;
    private readonly ScraperOptions _options;


    public Scraper(ILogger<Scraper> logger, IOptions<ScraperOptions> options, ITwitchAuthService twitchAuthService,
        IIgdbClient igdbClient, IMaxIdChecker maxIdChecker)
    {
        _logger = logger;
        _options = options.Value;
        _igdbClient = igdbClient;
        _maxIdChecker = maxIdChecker;
    }

    public async Task<IEnumerable<T>> Scrape<T>(CancellationToken cancellationToken = default)
    {
        using var l = _logger.With("CurrentlyScraped", typeof(T).Name);

        var maxConcurrentConnections = _options.RpsLimit;
        var delayBetweenRequestsInMilliseconds = _options.DelayBetweenRequestsInMilliseconds;
        var batchSize = _options.BathSize;

        var maxId = await _maxIdChecker.GetMaxId<T>(cancellationToken);
        if (maxId == -1)
        {
            _logger.LogWarning($"Aborting scraping {typeof(T)}, max ID couldn't be specified.");
            return Array.Empty<T>();
        }

        var totalCount = await _maxIdChecker.GetCount<T>(cancellationToken);
        var bag = new ConcurrentBag<T>();

        long i = 1;
        var loop = true;
        long currentTotal = 0;
        var sw = new Stopwatch();
        while (loop)
        {
            sw.Restart();
            var tasks = new List<Task<int>>(maxConcurrentConnections);
            for (var j = 0; j < maxConcurrentConnections; j++)
            {
                var start = i;
                var end = start + batchSize;
                if (end > maxId)
                {
                    end = maxId;
                }

                tasks.Add(ScrapeSingleBatchWithRetry<T>(bag, start, end));

                i = end + 1;
                if (i >= maxId)
                {
                    loop = false;
                    break;
                }
            }

            var results = await Task.WhenAll(tasks);
            currentTotal += results.Sum();

            PrintProgress<T>(currentTotal, totalCount);
            var requestsTime = sw.ElapsedMilliseconds;
            await WaitIfNeeded(requestsTime, delayBetweenRequestsInMilliseconds);
        }


        if (bag.Count == totalCount)
            _logger.LogInformation($"Obtained {bag.Count} items, when expected {totalCount} of type: {typeof(T)}.");
        else
            _logger.LogWarning(
                $"Obtained {bag.Count} downloadedCount, when expected {totalCount} of type: {typeof(T)}.");

        return bag;
    }

    private async Task<int> ScrapeSingleBatchWithRetry<T>(ConcurrentBag<T> bag, long start, long end)
    {
        const int maxAttempts = 2;
        var attempts = 0;
        do
        {
            try
            {
                attempts++;
                return await ScrapeSingleBatch<T>(bag, start, end);
            }
            catch (IgdbClientException ex) when (ex is
                                                 {
                                                     Reason: IgdbClientExceptionReason.StatusCodeNotSuccess,
                                                     StatusCode: 429
                                                 })
            {
                if (attempts >= maxAttempts)
                    throw;

                await Task.Delay((attempts + 1) * 100);
            }
        } while (true);
    }

    private async Task<int> ScrapeSingleBatch<T>(ConcurrentBag<T> bag, long start, long end)
    {
        var requestBodyContent = $"fields *; where id >= {start} & id <= {end}; limit {_options.BathSize};";

        var igdbEntities = await _igdbClient.FetchData<T>(requestBodyContent);
        var count = igdbEntities.Count();
        _logger.LogInformation($"Obtained {count} entities of type '{typeof(T)}'");

        foreach (var igdbEntity in igdbEntities)
        {
            bag.Add(igdbEntity);
        }

        return count;
    }

    private async Task WaitIfNeeded(long requestsTime, int delayBetweenRequestsInMilliseconds)
    {
        var diff = delayBetweenRequestsInMilliseconds - requestsTime;
        if (diff > 0)
        {
            var wait = (int) diff + 50;
            _logger.LogDebug($"Request batch took {requestsTime}ms to complete. Waiting additional {wait}ms.");
            await Task.Delay(wait);
        }
    }

    private void PrintProgress<T>(long currentTotal, long totalCount)
    {
        var progress = (double) currentTotal / totalCount * 100;
        _logger.LogInformation($"Total progress of {typeof(T)} {Math.Round(progress, 2)}% {currentTotal}/{totalCount}");
    }
}