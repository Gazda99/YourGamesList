using System;

namespace YourGamesList.Contracts.Requests.Lists;

public record TransferGameListEntryRequestBody
{
    /// <summary>
    /// The list the entry should be copied or moved to.
    /// </summary>
    public required Guid TargetListId { get; init; }

    /// <summary>
    /// When true the entry is moved to the target list, otherwise it is copied.
    /// </summary>
    public required bool Move { get; init; }
}
