using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace YourGamesList.Database.Entities;

public class GameListEntry
{
    public Guid Id { get; init; }
    public required DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset? LastModifiedDate { get; set; }

    [StringLength(500)] public string Description { get; set; } = string.Empty;
    public bool IsStarred { get; set; }
    [Range(0, 5)] public byte? Rating { get; set; }
    public CompletionStatus CompletionStatus { get; set; } = CompletionStatus.Unspecified;

    public virtual ICollection<OwnershipInfo> OwnershipInfo { get; } = [];

    public virtual Game Game { get; set; } = null!;
    public virtual long GameId { get; set; }

    public virtual GamesList GamesList { get; set; } = null!;
    public virtual Guid GamesListId { get; set; }
}