using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace YourGamesList.Database.Entities;

public class GamesList
{
    public Guid Id { get; init; }

    [StringLength(500)] public string Desc { get; set; } = string.Empty;
    [StringLength(50)] public required string Name { get; set; }
    public bool IsPublic { get; set; } = true;
    public bool CanBeDeleted { get; set; } = true;
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset? LastModifiedDate { get; set; }

    public virtual ICollection<GameListEntry> Entries { get; } = [];

    public virtual User User { get; set; } = null!;
    public virtual Guid UserId { get; set; }
}