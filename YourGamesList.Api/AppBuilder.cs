using System;
using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Refit;
using Serilog;
using YourGamesList.Api.Model.Requests.Users;
using YourGamesList.Api.Services.ControllerModelValidators;
using YourGamesList.Api.Services.CorrelationId;
using YourGamesList.Api.Services.Igdb;
using YourGamesList.Api.Services.Igdb.Options;
using YourGamesList.Api.Services.Twitch;
using YourGamesList.Api.Services.Twitch.Options;
using YourGamesList.Api.Telemetry;
using YourGamesList.Common.Caching;
using YourGamesList.Common.Http;
using YourGamesList.Common.Options.Validators;
using YourGamesList.Common.Telemetry;
using YourGamesList.Database;
using YourGamesList.Database.Options;

namespace YourGamesList.Api;

[ExcludeFromCodeCoverage]
public static class AppBuilder
{
    public static WebApplication GetApp(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddConfigAndVariables();

        //Logger
        builder.Logging.ClearProviders();
        builder.Host.AddLogger(builder.Configuration);

        builder.Services.AddControllers();
        builder.Services.AddOpenApi();
        
        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
        builder.Services.AddMemoryCache();

        //Telemetry
        builder.Services.AddTelemetry(builder.Configuration, TelemetryCustomConfiguration.CustomTracerConfiguration, TelemetryCustomConfiguration.CustomMeterConfiguration);

        //YGL services
        builder.Services.AddYourGamesListDatabase();
        builder.Services.AddSingleton<ICorrelationIdProvider, CorrelationIdProvider>();

        //Other services
        builder.Services.AddRequestModelValidators();

        builder.Services.AddKeyedSingleton<ICacheProvider, InMemoryCacheProvider>(CacheProviders.InMemory);


        builder.Services.AddTwitchAuthService();
        builder.Services.AddIgdbService();


        var app = builder.Build();
        return app;
    }

    private static IServiceCollection AddIgdbService(this IServiceCollection services)
    {
        services.AddOptionsWithFluentValidation<IgdbHttpClientOptions, IgdbHttpClientOptionsValidator>(IgdbHttpClientOptions.SectionName);

        services.AddRefitGeneratedClient<IIgdbApi>().ConfigureHttpClient((provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<IgdbHttpClientOptions>>();
                client.BaseAddress = new Uri(options.Value.BaseAddress);
            })
            .ConfigureLogging();

        services.AddScoped<IIgdbService, IgdbService>();
        services.AddScoped<IIgdbGamesService, IgdbGamesService>();

        return services;
    }

    private static IServiceCollection AddTwitchAuthService(this IServiceCollection services)
    {
        services.AddOptionsWithFluentValidation<TwitchAuthHttpClientOptions, TwitchAuthHttpClientOptionsValidator>(TwitchAuthHttpClientOptions.SectionName);
        services.AddOptionsWithFluentValidation<TwitchAuthOptions, TwitchAuthOptionsValidator>(TwitchAuthOptions.SectionName);

        services.AddRefitGeneratedClient<ITwitchAuthApi>().ConfigureHttpClient((provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<TwitchAuthHttpClientOptions>>();
                client.BaseAddress = new Uri(options.Value.BaseAddress);
            })
            .ConfigureLogging();

        services.AddScoped<TwitchAuthService>();
        services.AddScoped<ITwitchAuthService>(x =>
            new TwitchAuthServiceWithCaching(
                x.GetRequiredService<ILogger<TwitchAuthServiceWithCaching>>(),
                x.GetRequiredKeyedService<ICacheProvider>(CacheProviders.InMemory),
                x.GetRequiredService<TwitchAuthService>()
            ));

        return services;
    }

    private static IServiceCollection AddYourGamesListDatabase(this IServiceCollection services)
    {
        services.AddOptionsWithFluentValidation<YourGamesListDatabaseOptions, YourGamesListDatabaseOptionsValidator>(YourGamesListDatabaseOptions.SectionName);

        services.AddDbContextFactory<YourGamesListDbContext>();
        return services;
    }

    private static IServiceCollection AddRequestModelValidators(this IServiceCollection services)
    {
        //Essentials
        services
            .AddScoped<IBaseRequestValidator, BaseRequestValidator>()
            .AddScoped<IValidationFailedResultFactory, ValidationFailedResultFactory>()
            ;

        //Auth controller validators
        services
            .AddScoped<IValidator<UserRegisterRequest>, UserRegisterRequestValidator>()
            .AddScoped<IValidator<UserLoginRequest>, UserLoginRequestValidator>()
            .AddScoped<IValidator<UserDeleteRequest>, UserDeleteRequestValidator>()
            ;

        return services;
    }

    private static ConfigureHostBuilder AddLogger(this ConfigureHostBuilder host, IConfiguration configuration)
    {
        var logger = new LoggerConfiguration().ReadFrom.Configuration(configuration);
        global::Serilog.Log.Logger = logger.CreateLogger();
        host.UseSerilog();

        return host;
    }

    private static WebApplicationBuilder AddConfigAndVariables(this WebApplicationBuilder builder)
    {
        const string appsettingsFileName = "appsettings";
        const string serilogsettingsFileName = "serilogsettings";
        const string envVariableName = "ENV";

        builder.Configuration.AddEnvironmentVariables();

        var env = Environment.GetEnvironmentVariable(envVariableName);
        env = env?.ToLower();

        Console.WriteLine($"Using environment '{env}'");

        builder.Configuration.AddJsonFile($"{appsettingsFileName}.json", optional: false, reloadOnChange: true);
        builder.Configuration.AddJsonFile($"{appsettingsFileName}.{env}.json", optional: false, reloadOnChange: true);

#if DEBUG
        builder.Configuration.AddJsonFile($"{serilogsettingsFileName}.debug.json", optional: false, reloadOnChange: true);
#else
        builder.Configuration.AddJsonFile($"{serilogsettingsFileName}.{env}.json", optional: false, reloadOnChange: true);
#endif

        return builder;
    }
}