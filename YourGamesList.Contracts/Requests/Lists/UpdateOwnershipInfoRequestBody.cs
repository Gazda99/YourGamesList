using YourGamesList.Contracts;
using YourGamesList.Contracts.Dto;

namespace YourGamesList.Contracts.Requests.Lists;

public record UpdateOwnershipInfoRequestBody
{
    public Optional<bool?> IsLegit { get; init; }
    public Optional<PlatformDto?> Platform { get; init; }
    public Optional<GameDistributionDto?> GameDistribution { get; init; }
    public Optional<bool?> WasEmulated { get; init; }
    public Optional<EmulatorDto?> EmulatedOn { get; init; }
}
