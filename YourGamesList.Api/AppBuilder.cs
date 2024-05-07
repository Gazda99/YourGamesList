﻿using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using YourGamesList.Common.Options.Validators;
using YourGamesList.Common.Services.Serilog;
using YourGamesList.Common.Services.Swagger;
using YourGamesList.Services.Hltb.Options;
using YourGamesList.Services.Hltb.Services;
using YourGamesList.Services.Twitch.Options;
using YourGamesList.Services.Twitch.Services;


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
        builder.Services.AddValidatorsFromAssembly(typeof(TwitchAuthOptionsValidator).Assembly);
        builder.Services
            .AddOptionsWithFluentValidation<TwitchAuthOptions>(TwitchAuthOptions.OptionsName)
            .AddOptionsWithFluentValidation<TwitchAuthHttpClientOptions>(TwitchAuthHttpClientOptions.OptionsName);

        builder.Services.AddValidatorsFromAssembly(typeof(HltbHttpClientOptionsValidator).Assembly);
        builder.Services.AddOptionsWithFluentValidation<HltbHttpClientOptions>(HltbHttpClientOptions.OptionsName);

        //other services


        //http clients
        builder.AddHttpClients();

        //other services
        builder.Services.AddScoped<ITwitchAuthService, TwitchAuthService>();
        builder.Services.AddScoped<IHltbService, HltbService>();

        var app = builder.Build();

        return app;
    }
}