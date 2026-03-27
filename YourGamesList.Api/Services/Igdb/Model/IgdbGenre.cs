using System.Text.Json.Serialization;

namespace YourGamesList.Api.Services.Igdb.Model;

public class IgdbGenre
{
    [JsonPropertyName("name")] public string Name { get; init; } = string.Empty;
}