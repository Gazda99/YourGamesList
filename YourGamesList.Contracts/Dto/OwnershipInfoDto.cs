using System;

namespace YourGamesList.Contracts.Dto;

public class OwnershipInfoDto
{
    public required Guid Id { get; init; }
    public required DateTimeOffset CreatedDate { get; init; }
    public required DateTimeOffset? LastModifiedDate { get; init; }

    public required bool? IsLegit { get; init; }
    public required PlatformDto Platform { get; init; }
    public required GameDistributionDto GameDistribution { get; init; }
    public required bool WasEmulated { get; init; }
    public required EmulatorDto? EmulatedOn { get; init; }
}