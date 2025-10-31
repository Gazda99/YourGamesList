using System.Text.Json.Serialization;

namespace YourGamesList.Api.Services.Igdb.Model;

public class MultiQueryCount
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("count")] public int Count { get; set; }
}