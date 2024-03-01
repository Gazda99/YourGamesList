using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using YourGamesList.Common.Log;
using YourGamesList.Common.Services.TwitchAuth;

namespace YourGamesList.IgdbScraper;

public class IgdbScraperHostedService : BackgroundService
{
    private readonly ILogger<IgdbScraperHostedService> _logger;
    private readonly HttpClient _httpClient;
    private readonly ITwitchAuthService _twitchAuthService;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="httpClientFactory">Needs named client: "IgdbHttpClient"</param>
    /// <param name="twitchAuthService"></param>
    public IgdbScraperHostedService(ILogger<IgdbScraperHostedService> logger, IHttpClientFactory httpClientFactory,
        ITwitchAuthService twitchAuthService)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("IgdbHttpClient");
        _twitchAuthService = twitchAuthService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var l = _logger.WithCorrelationId();
        _logger.LogInformation("Starting IGDB Scraper service.");
        var twitchToken = await _twitchAuthService.ObtainAccessToken(stoppingToken);

        _logger.LogInformation("IGDB Scraper service finished.");
    }
}