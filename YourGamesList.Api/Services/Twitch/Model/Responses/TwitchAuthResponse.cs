using System.Text.Json.Serialization;

namespace YourGamesList.Api.Services.Twitch.Model.Responses;

public class TwitchAuthResponse
{
    [JsonPropertyName("access_token")] public string? AccessToken { get; init; }
    [JsonPropertyName("expires_in")] public int ExpiresIn { get; init; }
    [JsonPropertyName("token_type")] public string? TokenType { get; init; }
}