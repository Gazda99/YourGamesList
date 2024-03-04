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


    public IgdbScraperHostedService(
        ILogger<IgdbScraperHostedService> logger,
        ITwitchAuthService twitchAuthService,
        IMaxIdChecker maxIdChecker)
    {
        _logger = logger;
        _twitchAuthService = twitchAuthService;
        _maxIdChecker = maxIdChecker;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var l = _logger.WithCorrelationId();
        _logger.LogInformation("Starting IGDB Scraper service.");
        var twitchToken = await _twitchAuthService.ObtainAccessToken(stoppingToken);

        var x = await _maxIdChecker.GetCount<Game>(_twitchAuthService.GetClientId(), twitchToken.AccessToken);
        _logger.LogInformation($"Max Id for Game is {x}");

        _logger.LogInformation("IGDB Scraper service finished.");
    }
}