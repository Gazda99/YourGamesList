using FluentValidation;
using YourGamesList.Common.Options.Validators;
using YourGamesList.Common.Services.Serilog;
using YourGamesList.Services.Igdb.HostedServices;
using YourGamesList.Services.Igdb.Options;
using YourGamesList.Services.Igdb.Services;
using YourGamesList.Services.Twitch.Options;
using YourGamesList.Services.Twitch.Services;

namespace YourGamesList.Application.IgdbScraper;

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
        builder.Services.AddValidatorsFromAssembly(typeof(TwitchAuthOptionsValidator).Assembly);
        builder.Services
            .AddOptionsWithFluentValidation<TwitchAuthOptions>(TwitchAuthOptions.OptionsName)
            .AddOptionsWithFluentValidation<TwitchAuthHttpClientOptions>(TwitchAuthHttpClientOptions.OptionsName);

        builder.Services.AddValidatorsFromAssembly(typeof(IgdbHttpClientOptions).Assembly);
        builder.Services
            .AddOptionsWithFluentValidation<IgdbHttpClientOptions>(IgdbHttpClientOptions.OptionsName)
            .AddOptionsWithFluentValidation<ScraperOptions>(ScraperOptions.OptionsName);

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