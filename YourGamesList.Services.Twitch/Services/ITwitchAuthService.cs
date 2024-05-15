namespace YourGamesList.Services.Twitch.Services;

public interface ITwitchAuthService
{
    Task<string> ObtainAccessToken(CancellationToken token = default);

    string GetClientId();
}