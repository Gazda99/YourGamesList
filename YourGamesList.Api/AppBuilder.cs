using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using YourGamesList.Api.Services.CorrelationId;
using YourGamesList.Api.Telemetry;
using YourGamesList.Common.Telemetry;

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
        builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
        
        //Telemetry
        builder.Services.AddTelemetry(builder.Configuration, TelemetryCustomConfiguration.CustomTracerConfiguration, TelemetryCustomConfiguration.CustomMeterConfiguration);
        
        //YGL services
        builder.Services.AddSingleton<ICorrelationIdProvider, CorrelationIdProvider>();

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