namespace YourGamesList.Common.Services.TwitchAuth;

public interface ITwitchAuthService
{
    Task<TwitchAuthResult> ObtainAccessToken(CancellationToken token = default);

    string GetClientId();
}