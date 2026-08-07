using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using YourGamesList.Api.Services.CorrelationId;
using YourGamesList.Api.Telemetry;
using YourGamesList.Api.Telemetry.Options;
using YourGamesList.Common.Options.Validators;
using ExportProcessorType = OpenTelemetry.ExportProcessorType;

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
        builder.Services.AddTelemetry(builder.Configuration);
        
        //YGL services
        builder.Services.AddSingleton<ICorrelationIdProvider, CorrelationIdProvider>();

        var app = builder.Build();
        return app;
    }
    private static IServiceCollection AddTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptionsWithFluentValidation<TelemetryConfigurationOptions, TelemetryConfigurationOptionsValidator>(
            TelemetryConfigurationOptions.SectionName);

        var telemetryOptions = configuration.GetSection(TelemetryConfigurationOptions.SectionName).Get<TelemetryConfigurationOptions>()
                               ?? throw new InvalidOperationException("TelemetryConfiguration section is missing or invalid");

        var serviceName = telemetryOptions.ServiceName;
        var tracesEndpoint = telemetryOptions.OtlpExporterTracesEndpoint;
        var metricsEndpoint = telemetryOptions.OtlpExporterMetricsEndpoint;
        var tracesStreamName = telemetryOptions.TracesStreamName;
        var authHeader = $"Authorization=Basic {telemetryOptions.ApiSecret}";

        services.AddOpenTelemetry()
            .ConfigureResource(builder => builder.AddService(serviceName: serviceName, serviceInstanceId: Environment.MachineName))
            .WithTracing(builder => builder
                .AddHttpClientInstrumentation(options =>
                {
                    options.EnrichWithHttpRequestMessage += TelemetryConfigurator.HttpClientInstrumentationTracingEnrichWithBetterDisplayName;
                })
                .AddAspNetCoreInstrumentation()
                .AddYourGamesListActivitySources()
                .AddOtlpExporter(o =>
                {
                    o.Protocol = OtlpExportProtocol.HttpProtobuf;
                    o.Endpoint = new Uri(tracesEndpoint);
                    o.Headers = $"{authHeader},stream-name={tracesStreamName}";
                    o.ExportProcessorType = ExportProcessorType.Simple;
                })
            )
            .WithMetrics(builder => builder
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation()
                .AddHttpClientInstrumentation()
                .AddYourGamesListMeters()
                .AddOtlpExporter(o =>
                {
                    o.Protocol = OtlpExportProtocol.HttpProtobuf;
                    o.Endpoint = new Uri(metricsEndpoint);
                    o.Headers = $"{authHeader}";
                    o.ExportProcessorType = ExportProcessorType.Simple;
                })
            );

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