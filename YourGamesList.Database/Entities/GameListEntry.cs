using System;
using System.ComponentModel.DataAnnotations;

namespace YourGamesList.Database.Entities;

public class GameListEntry
{
    public Guid Id { get; init; }

    public virtual Game Game { get; set; } = null!;

    [StringLength(500)] public string Desc { get; set; } = string.Empty;
    public Platform[] Platforms { get; set; } = [];
    public GameDistribution[] GameDistributions { get; set; } = [];
    public bool IsStarred { get; set; }
    [Range(0, 5)] public byte Rating { get; set; }
    public CompletionStatus CompletionStatus { get; set; }

    public virtual GamesList GamesList { get; set; } = null!;
    public Guid GamesListId { get; set; }
}