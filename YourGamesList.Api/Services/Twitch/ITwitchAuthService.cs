using System.Threading.Tasks;
using YourGamesList.Api.Services.Twitch.Model;
using YourGamesList.Common;

namespace YourGamesList.Api.Services.Twitch;

public interface ITwitchAuthService
{
    /// <summary>
    /// Returns the client ID.
    /// </summary>
    string GetClientId();

    /// <summary>
    /// Gets an access token from Twitch.
    /// </summary>
    Task<ValueResult<TwitchToken>> GetAccessToken();
}