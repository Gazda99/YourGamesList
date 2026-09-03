using System.Text.Json.Serialization;

namespace YourGamesList.Api.Services.Igdb.Model;

public class IgdbCover
{
    [JsonPropertyName("id")] public long Id { get; init; }

    [JsonPropertyName("image_id")] public string ImageId { get; init; } = string.Empty;
    [JsonPropertyName("image_type")] public IgdbImageType ImageType { get; init; } = new IgdbImageType();
    [JsonPropertyName("url")] public string Url { get; init; } = string.Empty;
}