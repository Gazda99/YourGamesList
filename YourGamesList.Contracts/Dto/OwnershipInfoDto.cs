using System;

namespace YourGamesList.Contracts.Dto;

public class OwnershipInfoDto
{
    public required Guid Id { get; set; }
    public required bool? IsLegit { get; set; }
    public required PlatformDto Platform { get; set; }
    public required GameDistributionDto GameDistribution { get; set; }
    public required bool WasEmulated { get; set; }
    public required EmulatorDto? EmulatedOn { get; set; }
}