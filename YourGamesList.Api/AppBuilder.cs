using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Options;
using YourGamesList.Common.Options;
using YourGamesList.Common.Options.Validators;
using YourGamesList.Common.Services.Serilog;
using YourGamesList.Common.Services.Swagger;


namespace YourGamesList.Api;

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
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerDefinitions();

        builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
        builder.Services.AddServerTiming();

        //options
        builder.Services.AddValidatorsFromAssembly(typeof(AppBuilder).Assembly);
        // builder.Services.AddValidatorsFromAssembly(typeof(AppBuilder).Assembly);
        builder.Services
            .AddOptionsWithFluentValidation<TwitchAuthOptions>(TwitchAuthOptions.OptionsName)
            .AddOptionsWithFluentValidation<TwitchAuthHttpClientOptions>(TwitchAuthHttpClientOptions.OptionsName)
            .AddOptionsWithFluentValidation<HltbHttpClientOptions>(HltbHttpClientOptions.OptionsName)
            ;

        //other services


        //http clients
        builder.AddHttpClients();

        //other services

        var app = builder.Build();

        return app;
    }
}