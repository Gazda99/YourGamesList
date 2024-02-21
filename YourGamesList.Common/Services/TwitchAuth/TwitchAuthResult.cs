namespace YourGamesList.Common.Services.TwitchAuth;

public class TwitchAuthResult
{
    public TwitchAuthResult(string accessToken)
    {
        AccessToken = accessToken;
    }

    public string AccessToken { get; init; }
}