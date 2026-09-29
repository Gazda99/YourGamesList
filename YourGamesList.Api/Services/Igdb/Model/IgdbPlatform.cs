using System.Text.Json.Serialization;

namespace YourGamesList.Api.Services.Igdb.Model;

public class IgdbPlatform
{
    [JsonPropertyName("name")] public string Name { get; init; } = string.Empty;
}