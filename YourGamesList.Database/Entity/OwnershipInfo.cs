using System;
using System.Diagnostics.CodeAnalysis;

namespace YourGamesList.Database.Entity;

[ExcludeFromCodeCoverage]
public class OwnershipInfo
{
    public Guid Id { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ModifiedAt { get; set; }

    public bool? IsLegit { get; set; }
    public Platform Platform { get; set; } = Platform.Unspecified;
    public GameDistribution GameDistribution { get; set; } = GameDistribution.Unspecified;
    public bool WasEmulated { get; set; } = false;
    public Emulator? EmulatedOn { get; set; }

    public Guid GameListEntryId { get; set; }
    public virtual GamesListEntry GameListEntry { get; set; } = null!;
}