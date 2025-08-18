using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Refit;
using Serilog;
using YourGamesList.Api.Services.Twitch;
using YourGamesList.Api.Services.Twitch.Options;
using YourGamesList.Common.Http;
using YourGamesList.Common.Options.Validators;

namespace YourGamesList.Api.AppBuilders;

[ExcludeFromCodeCoverage]
public static partial class AppBuilder
{
    public static WebApplication GetApp(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddConfigAndVariables();

        //Logger
        builder.Logging.ClearProviders();
        builder.Host.AddLogger(builder.Configuration);

        builder.Services.AddControllers();

        //Swagger
        builder.Services.AddSwaggerGen();
        builder.Services.AddEndpointsApiExplorer();


        //Base services
        builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
        builder.Services.AddMemoryCache();

        //Other services
        builder.Services.AddRequestModelValidators();

        builder.Services.AddTwitchAuthService();

        var app = builder.Build();
        return app;
    }

    private static ConfigureHostBuilder AddLogger(this ConfigureHostBuilder host, IConfiguration configuration)
    {
        var logger = new LoggerConfiguration().ReadFrom.Configuration(configuration);
        global::Serilog.Log.Logger = logger.CreateLogger();
        host.UseSerilog();

        return host;
    }

    private static IServiceCollection AddTwitchAuthService(this IServiceCollection services)
    {
        services.AddOptionsWithFluentValidation<TwitchAuthHttpClientOptions, TwitchAuthHttpClientOptionsValidator>(TwitchAuthHttpClientOptions.SectionName);
        services.AddOptionsWithFluentValidation<TwitchAuthOptions, TwitchAuthOptionsValidator>(TwitchAuthOptions.SectionName);

        services.AddRefitClient<ITwitchAuthApi>().ConfigureHttpClient((provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<TwitchAuthHttpClientOptions>>();
                client.BaseAddress = new Uri(options.Value.BaseAddress);
            })
            .ConfigureLogging();

        services.AddScoped<ITwitchAuthService, TwitchAuthService>();

        return services;
    }
}