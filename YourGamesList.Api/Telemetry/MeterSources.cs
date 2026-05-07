using OpenTelemetry.Metrics;

namespace YourGamesList.Api.Telemetry;

public static class MeterSources
{
    public const string UserManagerServiceTelemetryMeterName = "YourGamesList.Api.UserManagerService";
}

public static class MeterExtensions
{
    public static MeterProviderBuilder AddYourGamesListMeters(this MeterProviderBuilder builder)
    {
        builder.AddMeter([
            MeterSources.UserManagerServiceTelemetryMeterName
        ]);

        return builder;
    }
}