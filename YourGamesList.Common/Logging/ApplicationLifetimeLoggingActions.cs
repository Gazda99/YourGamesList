using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace YourGamesList.Common.Logging;

public static class ApplicationLifetimeLoggingActions
{
    public static IHostApplicationLifetime AddApplicationLifetimeActions(this IHostApplicationLifetime lifetime, IServiceProvider serviceProvider)
    {
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        lifetime.ApplicationStarted.Register(() => OnStarted(loggerFactory.CreateLogger($"{nameof(IHostApplicationLifetime)}.ApplicationStarted")));
        lifetime.ApplicationStopping.Register(() => OnStopping(loggerFactory.CreateLogger($"{nameof(IHostApplicationLifetime)}.ApplicationStopping")));
        lifetime.ApplicationStopped.Register(() => OnStopped(loggerFactory.CreateLogger($"{nameof(IHostApplicationLifetime)}.ApplicationStopped")));

        return lifetime;
    }

    private static void OnStarted(ILogger logger)
    {
        logger.LogInformation("Application starting.");
    }

    private static void OnStopping(ILogger logger)
    {
        logger.LogInformation("Application stopping.");
    }

    private static void OnStopped(ILogger logger)
    {
        logger.LogInformation("Application stopped.");
    }
}