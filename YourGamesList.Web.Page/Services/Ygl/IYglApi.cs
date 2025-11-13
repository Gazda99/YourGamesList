using System.Threading.Tasks;
using Refit;
using YourGamesList.Common.Http;
using YourGamesList.Common.Refit;
using YourGamesList.Web.Page.Services.Ygl.Model.Requests;
using YourGamesList.Web.Page.Services.Ygl.Model.Responses;

namespace YourGamesList.Web.Page.Services.Ygl;

public interface IYglApi : IHandlesHttpRefitException
{
    #region Auth

    [Post("/auth/register")]
    [Headers($"Accept: {ContentTypes.ApplicationJson}", $"Content-Type: {ContentTypes.ApplicationJson}")]
    Task<IApiResponse> Register([Body(BodySerializationMethod.Serialized)] UserRegisterRequest request);

    [Post("/auth/login")]
    [Headers($"Content-Type: {ContentTypes.ApplicationJson}")]
    Task<IApiResponse<LoginResponse>> Login([Body(BodySerializationMethod.Serialized)] UserLoginRequest request);

    [Post("/auth/delete")]
    [Headers($"Content-Type: {ContentTypes.ApplicationJson}")]
    Task<IApiResponse> Delete([Body(BodySerializationMethod.Serialized)] UserDeleteRequest request);

    #endregion

    #region SearchGames

    [Get("/ygl/games/search")]
    [Headers($"Accept: {ContentTypes.ApplicationJson}", $"Content-Type: {ContentTypes.ApplicationJson}")]
    Task<IApiResponse<SearchGamesResponse>> SearchGames(
        [Authorize("Bearer")] string userToken,
        [Body(BodySerializationMethod.Serialized)]
        SearchGamesRequest request
    );

    #endregion
}