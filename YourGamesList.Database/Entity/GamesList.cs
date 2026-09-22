using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace YourGamesList.Database.Entity;

public class GamesList
{
    public Guid Id { get; set; }

    public required DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ModifiedAt { get; set; }

    public required bool CanBeDeleted { get; set; }
    [StringLength(500)] public string? Description { get; set; }
    public required bool IsPrivate { get; set; }
    [StringLength(50)] public required string Name { get; set; }

    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;

    public virtual ICollection<GamesListEntry> GameListEntries { get; } = [];
}