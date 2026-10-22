using System;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Lists;
using YourGamesList.Contracts.Dto;

namespace YourGamesList.Api.Services.Lists.Model;

public class AddOwnershipInfoParams
{
    public required UserInformationToken UserInformation { get; init; }
    public required Guid GamesListId { get; init; }
    public required Guid GameListEntryId { get; init; }
    public required PlatformDto Platform { get; init; }
    public required GameDistributionDto GameDistribution { get; init; }
    public bool? IsLegit { get; init; }
    public bool WasEmulated { get; init; }
    public EmulatorDto? EmulatedOn { get; init; }
}

public static class AddOwnershipInfoParamsMappingExtensions
{
    public static AddOwnershipInfoParams ToParams(this AddOwnershipInfoRequest request) => new()
    {
        UserInformation = request.UserInformation,
        GamesListId = request.ListId,
        GameListEntryId = request.EntryId,
        Platform = request.Body.Platform,
        GameDistribution = request.Body.GameDistribution,
        IsLegit = request.Body.IsLegit,
        WasEmulated = request.Body.WasEmulated,
        EmulatedOn = request.Body.EmulatedOn
    };
}
