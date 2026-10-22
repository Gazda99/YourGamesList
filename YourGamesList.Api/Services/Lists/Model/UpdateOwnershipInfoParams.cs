using System;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Lists;
using YourGamesList.Contracts;
using YourGamesList.Contracts.Dto;

namespace YourGamesList.Api.Services.Lists.Model;

public class UpdateOwnershipInfoParams
{
    public required UserInformationToken UserInformation { get; init; }
    public required Guid GamesListId { get; init; }
    public required Guid GameListEntryId { get; init; }
    public required Guid OwnershipInfoId { get; init; }
    public Optional<bool?> IsLegit { get; init; }
    public Optional<PlatformDto?> Platform { get; init; }
    public Optional<GameDistributionDto?> GameDistribution { get; init; }
    public Optional<bool?> WasEmulated { get; init; }
    public Optional<EmulatorDto?> EmulatedOn { get; init; }
}

public static class UpdateOwnershipInfoParamsMappingExtensions
{
    public static UpdateOwnershipInfoParams ToParams(this UpdateOwnershipInfoRequest request) => new()
    {
        UserInformation = request.UserInformation,
        GamesListId = request.ListId,
        GameListEntryId = request.EntryId,
        OwnershipInfoId = request.OwnershipInfoId,
        IsLegit = request.Body.IsLegit,
        Platform = request.Body.Platform,
        GameDistribution = request.Body.GameDistribution,
        WasEmulated = request.Body.WasEmulated,
        EmulatedOn = request.Body.EmulatedOn
    };
}
