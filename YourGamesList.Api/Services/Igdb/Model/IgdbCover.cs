using System.Text.Json.Serialization;

namespace YourGamesList.Api.Services.Igdb.Model;

public class IgdbCover
{
    [JsonPropertyName("alpha_channel")] public bool AlphaChannel { get; init; }
    [JsonPropertyName("animated")] public bool Animated { get; init; }
    [JsonPropertyName("height")] public int Height { get; init; }
    [JsonPropertyName("image_id")] public string ImageId { get; init; } = "";
    [JsonPropertyName("url")] public string Url { get; init; } = "";
    [JsonPropertyName("width")] public int Width { get; init; }
}