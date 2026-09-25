using System.Diagnostics;
using System.Net.Http;
using YourGamesList.Common.Telemetry;

namespace YourGamesList.Common.UnitTests.Telemetry;

public class TelemetryConfiguratorTests
{
    [Test]
    public void HttpClientInstrumentationTracingEnrichWithBetterDisplayName_AppliesName()
    {
        // Arrange
        using var activity = new Activity("TestActivity");
        
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.yourgameslist.com/v1/games");

        var expectedDisplayName = "GET:https://api.yourgameslist.com/v1/games";

        // Act
        TelemetryConfigurator.HttpClientInstrumentationTracingEnrichWithBetterDisplayName(activity, request);

        // Assert
        Assert.That(activity.DisplayName, Is.EqualTo(expectedDisplayName));
    }
}