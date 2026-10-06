using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using YourGamesList.Database;

namespace YourGamesList.Database.Entity;

[ExcludeFromCodeCoverage]
public class GamesList
{
    public Guid Id { get; set; }

    public required DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ModifiedAt { get; set; }

    public required bool CanBeDeleted { get; set; }
    [StringLength(ValidationConstants.GamesList.MaximumDescriptionLength)] public string? Description { get; set; }
    public required bool IsPrivate { get; set; }
    [StringLength(ValidationConstants.GamesList.MaximumNameLength)] public required string Name { get; set; }

    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;

    public virtual ICollection<GamesListEntry> GamesListEntries { get; } = [];
}