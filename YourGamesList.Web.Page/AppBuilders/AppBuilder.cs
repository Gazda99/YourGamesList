using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
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
using YourGamesList.Web.Page.Services;
using YourGamesList.Web.Page.Services.LocalStorage;
using YourGamesList.Web.Page.Services.StaticStorage;
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

        // Static State
        builder.Services.AddStaticState();

        builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
        builder.Services.AddOptionsWithFluentValidation<UserLoginStateManagerOptions, UserLoginStateManagerOptionsValidator>(UserLoginStateManagerOptions
            .SectionName);
        builder.Services.AddScoped<IUserLoginStateManager, UserLoginStateManager>();
        builder.Services.AddScoped<IUserListsManager, UserListsManager>();

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

    //AI Generated
    private static IServiceCollection AddStaticState(this IServiceCollection services)
    {
        var assembly = Assembly.GetAssembly(typeof(AppBuilder));
        if (assembly == null)
        {
            throw new InvalidOperationException("Cannot add static state without an assembly!");
        }

        var staticStateImplementations = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false })
            .Select(t => new
            {
                ConcreteType = t,
                InterfaceType = t.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IStaticState<>))
            })
            .Where(x => x.InterfaceType != null)
            .Where(x => x.ConcreteType != typeof(LoggingStaticStateDecorator<>));

        var logger = services.BuildServiceProvider().GetService<ILogger<Program>>();

        logger?.LogInformation($"Found '{staticStateImplementations.Count()}' of {typeof(IStaticState<>).Name} interfaces");

        foreach (var item in staticStateImplementations)
        {
            logger?.LogInformation($"Registering {typeof(IStaticState<>).Name} with logging decorator for type '{item.ConcreteType.Name}'.");
            // Step A: Register the concrete class itself (e.g., AvailableQueryArgumentsState)
            //Register concrete tyoe
            services.AddScoped(item.ConcreteType);

            // Step B: Register the Interface with a Factory that creates the Decorator
            services.AddScoped(item.InterfaceType!, provider =>
            {
                // 1. Get the specific generic argument (TState)
                var genericArg = item.InterfaceType!.GetGenericArguments()[0];

                // 2. Define the specific Decorator type (LoggingStaticStateDecorator<TState>)
                var specificDecoratorType = typeof(LoggingStaticStateDecorator<>)
                    .MakeGenericType(genericArg);

                // 3. Get the instance of the concrete inner class we registered in Step A
                var innerService = provider.GetRequiredService(item.ConcreteType);

                // 4. Create the Decorator instance. 
                // ActivatorUtilities is magic: it injects 'innerService' where it fits, 
                // and automatically resolves 'ILogger' from the provider for you.
                return ActivatorUtilities.CreateInstance(provider, specificDecoratorType, innerService);
            });
        }

        return services;
    }
}