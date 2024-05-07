using Igdb.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using YourGamesList.Common.Log;
using YourGamesList.Services.Igdb.Services;

namespace YourGamesList.Services.Igdb.HostedServices;

public class IgdbScraperHostedService : BackgroundService
{
    private readonly ILogger<IgdbScraperHostedService> _logger;
    private readonly IScraper _scraper;

    public IgdbScraperHostedService(
        ILogger<IgdbScraperHostedService> logger,
        IScraper scraper)
    {
        _logger = logger;
        _scraper = scraper;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var l = _logger.WithCorrelationId();
        _logger.LogInformation("Starting IGDB Scraper service.");

        var res = await _scraper.Scrape<Game>(stoppingToken);
        _logger.LogInformation(string.Join("\n", res.Select(x => x.Name)));

        _logger.LogInformation("IGDB Scraper service finished.");

        //TODO: stop application after this method finishes
    }
}