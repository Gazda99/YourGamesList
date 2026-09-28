using System.Diagnostics.CodeAnalysis;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace YourGamesList.Api.Telemetry;

[ExcludeFromCodeCoverage]
public static class TelemetryCustomConfiguration
{
    public static TracerProviderBuilder CustomTracerConfiguration(this TracerProviderBuilder builder)
    {
        builder.AddSource([]);
        return builder;
    }

    public static MeterProviderBuilder CustomMeterConfiguration(this MeterProviderBuilder builder)
    {
        builder.AddMeter([]);
        return builder;
    }
}