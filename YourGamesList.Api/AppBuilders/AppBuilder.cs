﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Refit;
using Serilog;
using YourGamesList.Api.ModelBinders;
using YourGamesList.Api.Services.Auth;
using YourGamesList.Api.Services.Auth.Options;
using YourGamesList.Api.Services.Igdb;
using YourGamesList.Api.Services.Igdb.Options;
using YourGamesList.Api.Services.Scraper;
using YourGamesList.Api.Services.Scraper.Options;
using YourGamesList.Api.Services.Twitch;
using YourGamesList.Api.Services.Twitch.Options;
using YourGamesList.Api.Services.Ygl;
using YourGamesList.Common.Caching;
using YourGamesList.Common.Http;
using YourGamesList.Common.Options.Validators;
using YourGamesList.Database;
using YourGamesList.Database.Options;

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

        builder.Services.AddControllers(options =>
        {
            options.ModelBinderProviders.Insert(0, new JwtUserInformationModelBinderProvider());
        });
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
        {
            o.RequireHttpsMetadata = false;
            o.TokenValidationParameters = new TokenValidationParameters()
            {
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration[$"{TokenAuthOptions.SectionName}:{nameof(TokenAuthOptions.JwtSecret)}"] ?? throw new NullReferenceException("JwtSecret is not set in appsettings"))),
                ValidIssuer = builder.Configuration[$"{TokenAuthOptions.SectionName}:{nameof(TokenAuthOptions.Issuer)}"],
                ValidateAudience = false
            };
        });


        //Swagger
        builder.Services.AddSwagger();

        //Base services
        builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
        builder.Services.AddMemoryCache();
        builder.Services.AddKeyedSingleton<ICacheProvider, InMemoryCacheProvider>(CacheProviders.InMemory);

        //Other services
        builder.Services.AddRequestModelValidators();

        builder.Services.AddAuth();
        builder.Services.AddDatabases();

        builder.Services.AddTwitchAuthService();
        builder.Services.AddIgdbService();

        builder.Services.AddScraper();

        builder.Services.AddScoped<IListsService, ListsService>();

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

    private static IServiceCollection AddAuth(this IServiceCollection services)
    {
        services.AddOptionsWithFluentValidation<TokenAuthOptions, TokenAuthOptionsValidator>(TokenAuthOptions.SectionName);
        services.AddOptionsWithFluentValidation<PasswordValidatorOptions, PasswordValidatorOptionsValidator>(PasswordValidatorOptions.SectionName);
        services.AddOptionsWithFluentValidation<ApiKeysOptions, ApiKeysOptionsValidator>(ApiKeysOptions.SectionName);

        services.AddSingleton<ITokenProvider, TokenProvider>();
        services.AddSingleton<IPasswordValidator, PasswordValidator>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        services.AddScoped<IUserManagerService, UserManagerService>();

        return services;
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

        services.AddScoped<TwitchAuthService>();
        services.AddScoped<ITwitchAuthService>(x =>
            new TwitchAuthServiceWithCaching(
                x.GetRequiredService<ILogger<TwitchAuthServiceWithCaching>>(),
                x.GetRequiredKeyedService<ICacheProvider>(CacheProviders.InMemory),
                x.GetRequiredService<TwitchAuthService>()
            ));

        return services;
    }

    private static IServiceCollection AddIgdbService(this IServiceCollection services)
    {
        services.AddOptionsWithFluentValidation<IgdbHttpClientOptions, IgdbHttpClientOptionsValidator>(IgdbHttpClientOptions.SectionName);
        services.AddRefitClient<IIgdbApi>().ConfigureHttpClient((provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<IgdbHttpClientOptions>>();
                client.BaseAddress = new Uri(options.Value.BaseAddress);
            })
            .ConfigureLogging();

        services.AddScoped<IIgdbService, IgdbService>();
        services.AddScoped<IGamesIgdbService, GamesIgdbService>();

        return services;
    }

    private static IServiceCollection AddDatabases(this IServiceCollection services)
    {
        services.AddOptionsWithFluentValidation<YourGamesListDatabaseOptions, YourGamesListDatabaseOptionsValidator>(YourGamesListDatabaseOptions.SectionName);

        services.AddDbContextFactory<YglDbContext>();
        return services;
    }

    private static IServiceCollection AddScraper(this IServiceCollection services)
    {
        services.AddOptionsWithFluentValidation<ScraperOptions, ScraperOptionsValidator>(ScraperOptions.SectionName);

        services.AddScoped<IScraperCache, ScraperCache>();
        services.AddScoped<IScraperService, ScraperService>();
        services.AddScoped<IBackgroundScraper, BackgroundScraper>();

        return services;
    }
}