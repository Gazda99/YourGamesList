using System;
using YourGamesList.Contracts.Dto;

namespace YourGamesList.Contracts.Responses.HealthChecks;

public class HealthCheckResponse
{
    public required ServiceStatusDto[] Services { get; set; } = [];
    public required HealthCheckStatusDto Status { get; set; }
    public required DateTimeOffset CheckedAt { get; set; }
}