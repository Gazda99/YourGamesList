using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using YourGamesList.Api.Telemetry;
using YourGamesList.Api.Telemetry.Options;
using YourGamesList.Common.Options.Validators;

namespace YourGamesList.Api.AppBuilders;

public static partial class AppBuilder
{
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
                    options.EnrichWithHttpRequestMessage += TelemetryConfigurator.HttpClientInstrumentationTracingEnrichWithRefitMethodInfo;
                })
                .AddAspNetCoreInstrumentation()
                .AddEntityFrameworkCoreInstrumentation(options => { options.EnrichWithIDbCommand = TelemetryConfigurator.EnrichWithIDbCommand; })
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
}