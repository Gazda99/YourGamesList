namespace YourGamesList.Common.Options;

public class TwitchAuthOptions
{
    public const string OptionsName = nameof(TwitchAuthOptions);

    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string TwitchAuthEndpoint { get; set; } = string.Empty;
}