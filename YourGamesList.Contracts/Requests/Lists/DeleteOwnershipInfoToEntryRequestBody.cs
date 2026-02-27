using System;

namespace YourGamesList.Contracts.Requests.Lists;

public class DeleteOwnershipInfoToEntryRequestBody
{
    public required Guid ListEntryId { get; init; }
    public Guid[] OwnershipsToRemove { get; init; } = [];
}