using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MudBlazor;
using MudBlazor.Services;
using Refit;
using Serilog;
using YourGamesList.Common;
using YourGamesList.Common.Caching;
using YourGamesList.Common.Http;
using YourGamesList.Common.Options.Validators;
using YourGamesList.Web.Page.Services;
using YourGamesList.Web.Page.Services.Caching;
using YourGamesList.Web.Page.Services.Caching.LocalStorage;
using YourGamesList.Web.Page.Services.UserLoginStateManager;
using YourGamesList.Web.Page.Services.UserLoginStateManager.Options;
using YourGamesList.Web.Page.Services.Ygl;
using YourGamesList.Web.Page.Services.Ygl.Options;

namespace YourGamesList.Web.Page.AppBuilders;

[ExcludeFromCodeCoverage]
public static partial class AppBuilder
{
    public static WebApplication GetApp(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddConfigAndVariables();

        // Logger
        builder.Logging.ClearProviders();
        builder.Host.AddLogger(builder.Configuration);

        // Add MudBlazor services
        builder.Services.AddMudBlazorServices();

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        builder.Services.AddMemoryCache();
        builder.Services.AddKeyedSingleton<ICacheProvider, InMemoryCacheProvider>(CacheProviders.InMemory);
        builder.Services.AddKeyedScoped<ICacheProvider, LocalStorageCache>(WebPageCacheProviders.LocalStorage);

        builder.Services.AddSingleton<ICountriesService, CountriesService>();
        
        builder.Services.AddOptionsWithFluentValidation<UserLoginStateManagerOptions, UserLoginStateManagerOptionsValidator>(UserLoginStateManagerOptions
            .SectionName);
        builder.Services.AddScoped<IUserLoginStateManager, UserLoginStateManager>();
        builder.Services.AddScoped<IUserListsManager, UserListsManager>();
        builder.Services.AddScoped<IUserManager, UserManager>();

        builder.Services.AddYourGamesListApi();

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

    private static IServiceCollection AddYourGamesListApi(this IServiceCollection services)
    {
        services.AddOptionsWithFluentValidation<YglApiHttpClientOptions, YglApiHttpClientOptionsValidator>(YglApiHttpClientOptions.SectionName);

        services.AddRefitClient<IYglApiAuth>().ConfigureHttpClient((provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<YglApiHttpClientOptions>>();
                client.BaseAddress = new Uri(options.Value.BaseAddress);
            })
            .ConfigureLogging();
        services.AddRefitClient<IYglApiUsers>().ConfigureHttpClient((provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<YglApiHttpClientOptions>>();
                client.BaseAddress = new Uri(options.Value.BaseAddress);
            })
            .ConfigureLogging();
        services.AddRefitClient<IYglApiSearchGames>().ConfigureHttpClient((provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<YglApiHttpClientOptions>>();
                client.BaseAddress = new Uri(options.Value.BaseAddress);
            })
            .ConfigureLogging();
        services.AddRefitClient<IYglApiLists>().ConfigureHttpClient((provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<YglApiHttpClientOptions>>();
                client.BaseAddress = new Uri(options.Value.BaseAddress);
            })
            .ConfigureLogging();

        services.AddScoped<IYglApi, YglApi>();
        services.AddScoped<IYglAuthClient, YglAuthAuthClient>();
        services.AddScoped<IYglGamesClient, YglGamesClient>();
        services.AddScoped<IYglUsersClient, YglUsersClient>();
        services.AddScoped<IYglListsClient, YglListsClient>();

        return services;
    }

    private static IServiceCollection AddMudBlazorServices(this IServiceCollection services)
    {
        services.AddMudServices(config =>
        {
            config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopRight;
            config.SnackbarConfiguration.PreventDuplicates = false;
            config.SnackbarConfiguration.NewestOnTop = false;
            config.SnackbarConfiguration.ShowCloseIcon = true;
            config.SnackbarConfiguration.VisibleStateDuration = 5000;
            config.SnackbarConfiguration.HideTransitionDuration = 500;
            config.SnackbarConfiguration.ShowTransitionDuration = 500;
            config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
        });

        return services;
    }
}