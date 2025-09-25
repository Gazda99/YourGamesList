using System.Text.Json.Serialization;

namespace YourGamesList.Api.Services.Igdb.Model;

public class IgdbGame
{
    [JsonPropertyName("cover")] public IgdbCover Cover { get; init; } = new IgdbCover();
    [JsonPropertyName("first_release_date")] public long FirstReleaseDate { get; init; }
    [JsonPropertyName("game_type")] public IgdbGameType GameType { get; init; } = new IgdbGameType();
    [JsonPropertyName("genres")] public IgdbGenre[] Genres { get; init; } = [];
    [JsonPropertyName("id")] public long Id { get; init; }
    [JsonPropertyName("name")] public string Name { get; init; } = string.Empty;
    [JsonPropertyName("rating_count")] public int RatingCount { get; init; } = 0;
    [JsonPropertyName("storyline")] public string StoryLine { get; init; } = string.Empty;
    [JsonPropertyName("summary")] public string Summary { get; init; } = string.Empty;
}