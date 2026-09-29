using System.Text.Json.Serialization;

namespace YourGamesList.Api.Services.Igdb.Model;

public class IgdbReleaseDate
{
    [JsonPropertyName("date")] public long Date { get; init; }
    [JsonPropertyName("platform")] public IgdbPlatform Platform { get; init; } = new IgdbPlatform();
}