using System.Diagnostics;
using OpenTelemetry.Trace;

namespace YourGamesList.Api.Telemetry;

public static class ActivitySources
{
    public static class HealthCheck
    {
    }
}

public static class ActivitySourceExtensions
{
    public static TracerProviderBuilder AddYourGamesListActivitySources(this TracerProviderBuilder builder)
    {
        builder.AddSource([
        ]);

        return builder;
    }
}