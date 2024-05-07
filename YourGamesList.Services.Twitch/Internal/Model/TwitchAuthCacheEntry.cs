namespace YourGamesList.Services.Twitch.Internal.Model;

internal class TwitchAuthCacheEntry
{
    public string AccessToken { get; init; }
    public long ExpiryDate { get; init; }

    public TwitchAuthCacheEntry(string accessToken, long expiryDate)
    {
        AccessToken = accessToken;
        ExpiryDate = expiryDate;
    }
}