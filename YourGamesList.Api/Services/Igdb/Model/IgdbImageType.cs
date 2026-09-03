using System.Text.Json.Serialization;

namespace YourGamesList.Api.Services.Igdb.Model;

public class IgdbImageType
{
    [JsonPropertyName("name")] public string Name { get; init; } = string.Empty;
}