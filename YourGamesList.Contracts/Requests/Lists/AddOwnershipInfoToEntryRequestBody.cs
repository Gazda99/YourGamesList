using System;
using YourGamesList.Contracts.Dto;

namespace YourGamesList.Contracts.Requests.Lists;

public class AddOwnershipInfoToEntryRequestBody
{
    public required Guid ListEntryId { get; init; }
    public OwnershipToAddRequestPart[] OwnershipsToAdd { get; init; } = [];
}

public class OwnershipToAddRequestPart
{
    public PlatformDto? Platform { get; set; }
    public GameDistributionDto? GameDistribution { get; set; }
    public bool? IsLegit { get; set; }
    public bool? WasEmulated { get; set; }
    public EmulatorDto? EmulatedOn { get; set; }
}