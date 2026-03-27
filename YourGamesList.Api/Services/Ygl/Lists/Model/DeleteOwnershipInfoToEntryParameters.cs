using System;
using YourGamesList.Api.Model;

namespace YourGamesList.Api.Services.Ygl.Lists.Model;

public class DeleteOwnershipInfoToEntryParameters
{
    public required UserInformationToken UserInformation { get; init; }
    public Guid ListEntryId { get; init; }
    public Guid[] OwnershipsToRemove { get; init; } = [];
}