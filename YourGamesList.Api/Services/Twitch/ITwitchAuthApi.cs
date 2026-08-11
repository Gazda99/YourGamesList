using System.Net.Http;
using System.Threading.Tasks;
using Refit;
using YourGamesList.Api.Services.Twitch.Model.Responses;
using YourGamesList.Common.Http;
using YourGamesList.Common.Refit;

namespace YourGamesList.Api.Services.Twitch;

public interface ITwitchAuthApi : IHandlesHttpRefitException
{
    [Post("/oauth2/token")]
    [Headers($"Accept: {ContentTypes.ApplicationJson}", $"Content-Type: {ContentTypes.FormUrlEncoded}")]
    Task<IApiResponse<TwitchAuthResponse>> GetAccessToken(
        [Body(BodySerializationMethod.UrlEncoded)]
        FormUrlEncodedContent content
    );
}