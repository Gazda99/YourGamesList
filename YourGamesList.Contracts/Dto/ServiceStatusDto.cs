using System.Text.Json.Serialization;

namespace YourGamesList.Contracts.Dto;

public class ServiceStatusDto
{
    public required string Name { get; set; }
    public required HealthCheckStatusDto Status { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; set; }
}