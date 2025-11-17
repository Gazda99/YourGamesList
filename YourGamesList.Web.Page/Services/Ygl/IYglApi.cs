using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;
using YourGamesList.Common.Http;
using YourGamesList.Common.Refit;
using YourGamesList.Contracts.Dto;
using YourGamesList.Contracts.Requests.Games;
using YourGamesList.Contracts.Requests.Users;
using YourGamesList.Web.Page.Services.Ygl.Model.Responses;

namespace YourGamesList.Web.Page.Services.Ygl;

public interface IYglApi : IHandlesHttpRefitException
{
    #region Auth

    [Post("/users/auth/register")]
    [Headers($"Accept: {ContentTypes.ApplicationJson}", $"Content-Type: {ContentTypes.ApplicationJson}")]
    Task<IApiResponse<Guid>> Register([Body(BodySerializationMethod.Serialized)] AuthUserRegisterRequestBody request);

    [Post("/users/auth/login")]
    [Headers($"Content-Type: {ContentTypes.ApplicationJson}")]
    Task<IApiResponse<LoginResponse>> Login([Body(BodySerializationMethod.Serialized)] AuthUserLoginRequestBody request);

    [Post("/users/auth/delete")]
    [Headers($"Content-Type: {ContentTypes.ApplicationJson}")]
    Task<IApiResponse<Guid>> Delete([Body(BodySerializationMethod.Serialized)] AuthUserDeleteRequestBody request);

    #endregion

    #region SearchGames

    [Get("/games/ygl/search")]
    [Headers($"Accept: {ContentTypes.ApplicationJson}", $"Content-Type: {ContentTypes.ApplicationJson}")]
    Task<IApiResponse<List<GameDto>>> SearchGames(
        [Authorize("Bearer")] string userToken,
        [Body(BodySerializationMethod.Serialized)]
        SearchYglGamesRequestBody request
    );

    [Get("/games/ygl/paramsForSearching")]
    [Headers($"Accept: {ContentTypes.ApplicationJson}")]
    Task<IApiResponse<AvailableSearchQueryArgumentsResponse>> GetAvailableSearchParams(
        [Authorize("Bearer")] string userToken
    );

    #endregion

    #region Users

    [Post("/users/getSelf")]
    [Headers($"Accept: {ContentTypes.ApplicationJson}")]
    Task<IApiResponse<UserDto>> GetSelfUser([Authorize("Bearer")] string userToken);

    [Post("/users/update")]
    [Headers($"Content-Type: {ContentTypes.ApplicationJson}")]
    Task<IApiResponse<Guid>> UpdateUser([Authorize("Bearer")] string userToken,
        [Body(BodySerializationMethod.Serialized)]
        UserUpdateRequestBody request);

    #endregion
}