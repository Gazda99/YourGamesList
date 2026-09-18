using System;
using System.ComponentModel.DataAnnotations;

namespace YourGamesList.Database.Entity;

public class GamesListEntry
{
    public Guid Id { get; set; }

    public required DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ModifiedAt { get; set; }

    public required CompletionStatus CompletionStatus { get; set; }
    [StringLength(500)] public string? Description { get; set; }
    public required long IgdbId { get; set; }
    public byte? Rating { get; set; }

    public Guid GamesListId { get; set; }
    public GamesList GamesList { get; } = null!;
}