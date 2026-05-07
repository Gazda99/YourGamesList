using System.Diagnostics;
using OpenTelemetry.Trace;

namespace YourGamesList.Api.Telemetry;

public static class ActivitySources
{
    public static class HealthCheck
    {
        public static readonly ActivitySource HealthCheckActivitySource = new("HealthCheck");

        public static HealthCheckActivity Start(string name)
        {
            var activity = HealthCheckActivitySource.StartActivity(name);
            return new HealthCheckActivity(activity);
        }
    }
}

public static class ActivitySourceExtensions
{
    public static TracerProviderBuilder AddYourGamesListActivitySources(this TracerProviderBuilder builder)
    {
        builder.AddSource([
            ActivitySources.HealthCheck.HealthCheckActivitySource.Name
        ]);

        return builder;
    }
}