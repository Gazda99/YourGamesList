using OpenTelemetry.Metrics;

namespace YourGamesList.Api.Telemetry;

public static class MeterSources
{
}

public static class MeterExtensions
{
    public static MeterProviderBuilder AddYourGamesListMeters(this MeterProviderBuilder builder)
    {
        builder.AddMeter([
        ]);

        return builder;
    }
}