using System.Text.Json.Serialization;

namespace YourGamesList.Contracts.Dto;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HealthCheckStatusDto
{
    Healthy,
    Degraded,
    Unhealthy,
    HealthCheckNotCompleted
}