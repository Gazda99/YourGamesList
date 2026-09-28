using System.Diagnostics;
using System.Net.Http;
using YourGamesList.Common.Telemetry;

namespace YourGamesList.Common.UnitTests.Telemetry;

public class TelemetryConfiguratorTests
{
    [Test]
    public void HttpClientInstrumentationTracingEnrichWithBetterDisplayName_AppliesName()
    {
        //ARRANGE
        using var activity = new Activity("TestActivity");
        
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.yourgameslist.com/v1/games");

        var expectedDisplayName = "GET:https://api.yourgameslist.com/v1/games";

        //ACT
        TelemetryConfigurator.HttpClientInstrumentationTracingEnrichWithBetterDisplayName(activity, request);

        //ASSERT
        Assert.That(activity.DisplayName, Is.EqualTo(expectedDisplayName));
    }
}