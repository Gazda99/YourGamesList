using System;
using System.Diagnostics;
using System.Net.Http;

namespace YourGamesList.Common.Telemetry;

public static class TelemetryConfigurator
{
    /// <summary>
    /// Replaces the default span name with a formatted HTTP Method and Request URI.
    /// </summary>
    public static readonly Action<Activity, HttpRequestMessage> HttpClientInstrumentationTracingEnrichWithBetterDisplayName = (activity, httpRequestMessage) =>
    {
        activity.DisplayName = $"{httpRequestMessage.Method}:{httpRequestMessage.RequestUri}";
    };
}