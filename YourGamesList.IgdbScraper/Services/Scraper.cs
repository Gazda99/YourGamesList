using System.Collections.Concurrent;
using System.Diagnostics;
using Igdb.Model.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YourGamesList.Common.Http;
using YourGamesList.Common.Log;
using YourGamesList.Common.Services.TwitchAuth;
using YourGamesList.IgdbScraper.Options;

namespace YourGamesList.IgdbScraper.Services;

public class Scraper : IScraper
{
    private readonly ILogger<Scraper> _logger;
    private readonly HttpClient _httpClient;
    private readonly ITwitchAuthService _twitchAuthService;
    private readonly IMaxIdChecker _maxIdChecker;
    private readonly ScraperOptions _options;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="httpClientFactory">Needs named client: "IgdbHttpClient"</param>
    /// <param name="twitchAuthService"></param>
    /// <param name="maxIdChecker"></param>
    public Scraper(ILogger<Scraper> logger, IHttpClientFactory httpClientFactory,
        IOptions<ScraperOptions> options,
        ITwitchAuthService twitchAuthService, IMaxIdChecker maxIdChecker)
    {
        _logger = logger;
        _twitchAuthService = twitchAuthService;
        _httpClient = httpClientFactory.CreateClient("IgdbHttpClient");
        _options = options.Value;
        _maxIdChecker = maxIdChecker;
    }

    public async Task<IEnumerable<T>> Scrape<T>(CancellationToken cancellationToken = default)
    {
        using var l = _logger.With("CurrentlyScraped", typeof(T).Name);

        var accessToken = await _twitchAuthService.ObtainAccessToken(cancellationToken);
        
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

                tasks.Add(ScrapeSingleBatch<T>(bag, start, end, accessToken.ToString()));

                i = end + 1;
                if (i >= maxId)
                {
                    loop = false;
                    break;
                }
            }

            var results = await Task.WhenAll(tasks);
            //Interlocked.Add(ref currentTotal, results.Sum());
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


    private async Task<int> ScrapeSingleBatch<T>(ConcurrentBag<T> bag, long start, long end, string bearerToken)
    {
        var requestBodyContent = $"fields *; where id >= {start} & id <= {end}; limit {_options.BathSize};";

        var uri = IgdbEndpoints.GetEndpointBasedOnType<T>();

        var message = HttpRequestMessageBuilder.Create
            .WithMethod(HttpMethod.Post)
            .WithUri(uri, uriKind: UriKind.Relative)
            .WithHeaders(GetHeaderWithClientId(_twitchAuthService.GetClientId()))
            .WithBearerToken(bearerToken)
            .WithStringContent(requestBodyContent)
            .Build();

        _logger.LogInformation($"Scraping IDs from {start} to {end} ");

        var res = await _httpClient.SendAsync(message);
        var content = await res.Content.ReadAsStringAsync();

        var igdbEntities =
            JsonConvert.DeserializeObject<IEnumerable<T>>(content, IgdbSerializers.IgdbResponseSerializer)!;

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

    private static Dictionary<string, string> GetHeaderWithClientId(string clientId)
    {
        return new Dictionary<string, string>()
        {
            { "Client-ID", clientId }
        };
    }
}