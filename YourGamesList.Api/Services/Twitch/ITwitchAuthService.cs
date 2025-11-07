using System.Threading.Tasks;
using YourGamesList.Common;

namespace YourGamesList.Api.Services.Twitch;

public interface ITwitchAuthService
{
    /// <summary>
    /// Returns Client ID
    /// </summary>
    string GetClientId();

    /// <summary>
    /// Calls Twitch Auth Service in order to obtain Access Token
    /// </summary>
    Task<ValueResult<string>> GetAccessToken();
}