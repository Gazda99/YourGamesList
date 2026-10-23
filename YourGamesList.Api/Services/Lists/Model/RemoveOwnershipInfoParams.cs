using System;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Lists;

namespace YourGamesList.Api.Services.Lists.Model;

public class RemoveOwnershipInfoParams
{
    public required UserInformationToken UserInformation { get; init; }
    public required Guid GamesListId { get; init; }
    public required Guid GameListEntryId { get; init; }
    public required Guid OwnershipInfoId { get; init; }
}

public static class RemoveOwnershipInfoParamsMappingExtensions
{
    public static RemoveOwnershipInfoParams ToParams(this RemoveOwnershipInfoRequest request) => new()
    {
        UserInformation = request.UserInformation,
        GamesListId = request.ListId,
        GameListEntryId = request.EntryId,
        OwnershipInfoId = request.OwnershipInfoId
    };
}
