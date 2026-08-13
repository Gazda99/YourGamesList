using System.Text.Json.Serialization;

namespace YourGamesList.Api.Services.Igdb.Model;

public class IgdbGameType
{
    [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;
}