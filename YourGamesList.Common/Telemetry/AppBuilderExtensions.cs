using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using YourGamesList.Common.Options.Validators;
using YourGamesList.Common.Telemetry.Options;
using ExportProcessorType = OpenTelemetry.ExportProcessorType;

namespace YourGamesList.Common.Telemetry;

[ExcludeFromCodeCoverage]
public static class AppBuilderExtensions
{
    /// <summary>
    /// Requires <see cref="TelemetryConfigurationOptions"/> configured in the appsettings
    /// </summary>
    public static IServiceCollection AddTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        Func<TracerProviderBuilder, TracerProviderBuilder>? customTracerConfiguration = null,
        Func<MeterProviderBuilder, MeterProviderBuilder>? customMetricConfiguration = null
    )
    {
        services.AddOptionsWithFluentValidation<TelemetryConfigurationOptions, TelemetryConfigurationOptionsValidator>(TelemetryConfigurationOptions.SectionName);

        var telemetryOptions = configuration.GetSection(TelemetryConfigurationOptions.SectionName).Get<TelemetryConfigurationOptions>()
                               ?? throw new InvalidOperationException("TelemetryConfiguration section is missing or invalid");

        var serviceName = telemetryOptions.ServiceName;
        var tracesEndpoint = telemetryOptions.OtlpExporterTracesEndpoint;
        var metricsEndpoint = telemetryOptions.OtlpExporterMetricsEndpoint;
        var tracesStreamName = telemetryOptions.TracesStreamName;
        var authHeader = $"Authorization=Basic {telemetryOptions.ApiSecret}";

        services.AddOpenTelemetry()
            .ConfigureResource(builder => builder.AddService(serviceName: serviceName, serviceInstanceId: Environment.MachineName))
            .WithTracing(builder =>
            {
                builder
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.EnrichWithHttpRequestMessage += TelemetryConfigurator.HttpClientInstrumentationTracingEnrichWithBetterDisplayName;
                    })
                    .AddAspNetCoreInstrumentation()
                    .AddOtlpExporter(o =>
                    {
                        o.Protocol = OtlpExportProtocol.HttpProtobuf;
                        o.Endpoint = new Uri(tracesEndpoint);
                        o.Headers = $"{authHeader},stream-name={tracesStreamName}";
                        o.ExportProcessorType = ExportProcessorType.Simple;
                    });

                customTracerConfiguration?.Invoke(builder);
            })
            .WithMetrics(builder =>
            {
                builder
                    .AddAspNetCoreInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(o =>
                    {
                        o.Protocol = OtlpExportProtocol.HttpProtobuf;
                        o.Endpoint = new Uri(metricsEndpoint);
                        o.Headers = $"{authHeader}";
                        o.ExportProcessorType = ExportProcessorType.Simple;
                    });

                customMetricConfiguration?.Invoke(builder);
            });

        return services;
    }
}