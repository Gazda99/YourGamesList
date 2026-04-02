using System;
using System.Diagnostics;
using OpenTelemetry.Trace;
using YourGamesList.Contracts.Dto;

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

public readonly struct HealthCheckActivity : IDisposable
{
    private readonly Activity? _activity;

    public HealthCheckActivity(Activity? activity)
    {
        _activity = activity;
        _activity?.SetTag("is_health_check", "true");
    }

    public void SetHealthCheckStatus(HealthCheckStatusDto status)
    {
        _activity?.SetTag("health.status", status.ToString().ToUpper());
    }

    public void Dispose()
    {
        _activity?.Dispose();
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