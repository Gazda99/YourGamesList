using Igdb.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using YourGamesList.Common.Log;
using YourGamesList.Common.Services.TwitchAuth;
using YourGamesList.IgdbScraper.Services;

namespace YourGamesList.IgdbScraper;

public class IgdbScraperHostedService : BackgroundService
{
    private readonly ILogger<IgdbScraperHostedService> _logger;
    private readonly ITwitchAuthService _twitchAuthService;
    private readonly IMaxIdChecker _maxIdChecker;
    private readonly IScraper _scraper;


    public IgdbScraperHostedService(
        ILogger<IgdbScraperHostedService> logger,
        ITwitchAuthService twitchAuthService,
        IMaxIdChecker maxIdChecker,
        IScraper scraper)
    {
        _logger = logger;
        _twitchAuthService = twitchAuthService;
        _maxIdChecker = maxIdChecker;
        _scraper = scraper;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var l = _logger.WithCorrelationId();
        _logger.LogInformation("Starting IGDB Scraper service.");

        var res = await _scraper.Scrape<Platform>(stoppingToken);
        _logger.LogInformation(string.Join("\n", res.Select(x => x.Name)));

        _logger.LogInformation("IGDB Scraper service finished.");

        //TODO: stop application after this method finishes
    }
}