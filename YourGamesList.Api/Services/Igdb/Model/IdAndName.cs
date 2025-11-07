using System.Text.Json.Serialization;

namespace YourGamesList.Api.Services.Igdb.Model;

public class IdAndName
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("id")] public long Id { get; set; }
}