namespace YourGamesList.Contracts.Dto;

public class OwnershipInfoDto
{
    public bool? IsLegit { get; set; }
    public required PlatformDto Platform { get; set; } 
    public required GameDistributionDto GameDistribution { get; set; } 
    public bool WasEmulated { get; set; } = false;
    public EmulatorDto? EmulatedOn { get; set; }
}