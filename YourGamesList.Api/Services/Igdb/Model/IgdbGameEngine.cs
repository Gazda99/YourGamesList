using System.Text.Json.Serialization;

namespace YourGamesList.Api.Services.Igdb.Model;

public class IgdbGameEngine
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
}