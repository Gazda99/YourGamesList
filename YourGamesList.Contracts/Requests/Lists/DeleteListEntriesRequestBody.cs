using System;

namespace YourGamesList.Contracts.Requests.Lists;

public class DeleteListEntriesRequestBody
{
    public required Guid ListId { get; init; }
    public Guid[] EntriesToRemove { get; init; } = [];
}