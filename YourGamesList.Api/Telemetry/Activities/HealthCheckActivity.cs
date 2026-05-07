using System;
using System.Diagnostics;
using YourGamesList.Contracts.Dto;

namespace YourGamesList.Api.Telemetry.Activities;

public  class HealthCheckActivity : IDisposable
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