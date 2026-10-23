using YourGamesList.Contracts.Dto;

namespace YourGamesList.Contracts.Requests.Lists;

public record AddOwnershipInfoRequestBody
{
    public required PlatformDto Platform { get; init; }
    public required GameDistributionDto GameDistribution { get; init; }
    public bool? IsLegit { get; init; }
    public bool WasEmulated { get; init; }
    public EmulatorDto? EmulatedOn { get; init; }
}
