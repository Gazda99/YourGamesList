using System;
using System.Collections.Generic;

namespace YourGamesList.Database.Entities;

public class Game
{
    public Guid Id { get; init; }

    public long FirstReleaseDate { get; init; }
    public string GameType { get; set; } = string.Empty;
    public List<string> Genres { get; set; } = [];
    public required long IgdbGameId { get; set; }
    public string ImageId { get; set; } = string.Empty;
    public required string Name { get; set; } = string.Empty;
    public int RatingCount { get; set; }
    public string StoryLine { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public List<string> Themes { get; set; } = [];


    public virtual ICollection<GameListEntry> GameListEntries { get; set; } = [];
}