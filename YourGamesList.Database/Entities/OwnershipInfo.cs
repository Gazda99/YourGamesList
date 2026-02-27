using System;

namespace YourGamesList.Database.Entities;

public class OwnershipInfo
{
    public Guid Id { get; init; }
    public required DateTimeOffset CreatedDate { get; init; }
    public DateTimeOffset? LastModifiedDate { get; set; }

    public bool? IsLegit { get; set; } = null;
    public Platform Platform { get; set; }
    public GameDistribution GameDistribution { get; set; }
    public bool WasEmulated { get; set; } = false;
    public Emulator? EmulatedOn { get; set; } = null;

    public virtual GameListEntry GameListEntry { get; set; } = null!;
    public virtual Guid GameListEntryId { get; set; }
}