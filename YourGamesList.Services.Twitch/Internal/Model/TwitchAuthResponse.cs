namespace YourGamesList.Services.Twitch.Internal.Model;

internal class TwitchAuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public long ExpiresIn { get; set; }
    public string TokenType { get; set; } = string.Empty;
}