using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace YourGamesList.Api.Telemetry;

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