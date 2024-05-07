using YourGamesList.Services.Twitch.Model;

namespace YourGamesList.Services.Twitch.Services;

public interface ITwitchAuthService
{
    Task<TwitchAuthResult> ObtainAccessToken(CancellationToken token = default);

    string GetClientId();
}