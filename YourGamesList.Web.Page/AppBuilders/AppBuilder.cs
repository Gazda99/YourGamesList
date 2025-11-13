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
using YourGamesList.Common.Http;
using YourGamesList.Common.Options.Validators;
using YourGamesList.Web.Page.Services.LocalStorage;
using YourGamesList.Web.Page.Services.UserLoginState;
using YourGamesList.Web.Page.Services.UserLoginState.Options;
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

        // Fluxor
        // builder.Services.AddFluxor(x =>
        //     x.ScanAssemblies(typeof(YourGamesList.Web.Store.Marker).Assembly)
        // );

        builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
        builder.Services.AddOptionsWithFluentValidation<UserLoginStateManagerOptions, UserLoginStateManagerOptionsValidator>(UserLoginStateManagerOptions
            .SectionName);
        builder.Services.AddScoped<IUserLoginStateManager, UserLoginStateManager>();

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
        services.AddRefitClient<IYglApi>().ConfigureHttpClient((provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<YglApiHttpClientOptions>>();
                client.BaseAddress = new Uri(options.Value.BaseAddress);
            })
            .ConfigureLogging();

        services.AddScoped<IYglAuthClient, YglAuthAuthClient>();
        services.AddScoped<IYglGamesClient, YglGamesClient>();
        return services;
    }

    private static IServiceCollection AddMudBlazorServices(this IServiceCollection services)
    {
        services.AddMudServices(config =>
        {
            config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopRight;
            config.SnackbarConfiguration.PreventDuplicates = true;
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