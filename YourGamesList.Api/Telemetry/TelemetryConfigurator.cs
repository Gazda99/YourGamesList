using System;
using System.Diagnostics;
using System.Net.Http;
using Refit;

namespace YourGamesList.Api.Telemetry;

public static class TelemetryConfigurator
{
    public static readonly Action<Activity, HttpRequestMessage> HttpClientInstrumentationTracingEnrichWithBetterDisplayName = (activity, httpRequestMessage) =>
    {
        activity.DisplayName = $"{httpRequestMessage.Method}:{httpRequestMessage.RequestUri}";
    };

    public static readonly Action<Activity, HttpRequestMessage> HttpClientInstrumentationTracingEnrichWithRefitMethodInfo = (activity, httpRequestMessage) =>
    {
        const string tagName = "destination_name";

        if (!httpRequestMessage.Options.TryGetValue(new HttpRequestOptionsKey<RestMethodInfo>("Refit.RestMethodInfo"), out var refitMethodInfo))
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(refitMethodInfo.Name))
        {
            activity.SetTag(tagName, refitMethodInfo.Name);
        }
    };
}