using System;
using YourGamesList.Api.Model;
using YourGamesList.Contracts.Dto;

namespace YourGamesList.Api.Services.Ygl.Lists.Model;

public class AddOwnershipInfoToEntryParameters
{
    public required JwtUserInformation UserInformation { get; init; }
    public Guid ListEntryId { get; init; }
    public OwnershipsToAddParameter[] OwnershipsToAdd { get; init; } = [];
}

public class OwnershipsToAddParameter
{
    public PlatformDto? Platform { get; set; }
    public GameDistributionDto? GameDistribution { get; set; }
    public bool? IsLegit { get; set; }
    public bool? WasEmulated { get; set; }
    public EmulatorDto? EmulatedOn { get; set; }
}