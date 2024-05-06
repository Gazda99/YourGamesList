using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using YourGamesList.Common.Options;
using YourGamesList.Common.Options.Validators;
using YourGamesList.Common.Services.Serilog;
using YourGamesList.Common.Services.TwitchAuth;
using YourGamesList.IgdbScraper.Options;
using YourGamesList.IgdbScraper.Services.IgdbClient;
using YourGamesList.IgdbScraper.Services.MaxIdChecker;
using YourGamesList.IgdbScraper.Services.Scraper;

namespace YourGamesList.IgdbScraper;

public static class AppBuilder
{
    public static WebApplication GetApp(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddConfigFiles();

        //loggers
        builder.Logging.ClearProviders();
        builder.Host.AddLogger(builder.Configuration);

        builder.Services.AddControllers();
        builder.Services.AddMemoryCache();

        builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
        builder.Services.AddServerTiming();

        //options
        builder.Services.AddValidatorsFromAssembly(typeof(YourGamesList.Common.Options.TwitchAuthOptionsValidator)
            .Assembly);
        builder.Services.AddValidatorsFromAssembly(typeof(YourGamesList.IgdbScraper.Options.IgdbHttpClientOptions)
            .Assembly);
        builder.Services
            .AddOptionsWithFluentValidation<TwitchAuthOptions>(TwitchAuthOptions.OptionsName)
            .AddOptionsWithFluentValidation<TwitchAuthHttpClientOptions>(TwitchAuthHttpClientOptions.OptionsName)
            .AddOptionsWithFluentValidation<IgdbHttpClientOptions>(IgdbHttpClientOptions.OptionsName)
            .AddOptionsWithFluentValidation<ScraperOptions>(ScraperOptions.OptionsName)
            ;

        //http clients
        builder.AddHttpClients();

        //other services
        builder.Services.AddScoped<ITwitchAuthService, TwitchAuthService>();
        builder.Services.AddScoped<IIgdbClient, IgdbClient>();
        builder.Services.AddScoped<IMaxIdChecker, MaxIdChecker>();
        builder.Services.AddScoped<IScraper, Scraper>();
        builder.Services.AddHostedService<IgdbScraperHostedService>();

        var app = builder.Build();

        return app;
    }
}