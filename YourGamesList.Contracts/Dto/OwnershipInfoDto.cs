namespace YourGamesList.Contracts.Dto;

public class OwnershipInfoDto
{
    public bool? IsLegit { get; set; }
    public required PlatformDto Platform { get; set; }
    public required GameDistributionDto GameDistribution { get; set; }
    public required bool WasEmulated { get; set; }
    public EmulatorDto? EmulatedOn { get; set; }
}