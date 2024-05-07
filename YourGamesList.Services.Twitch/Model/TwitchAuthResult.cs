namespace YourGamesList.Services.Twitch.Model;

public class TwitchAuthResult
{
    public TwitchAuthResult(string accessToken)
    {
        AccessToken = accessToken;
    }

    public string AccessToken { get; init; }

    public override string ToString()
    {
        return AccessToken;
    }
}