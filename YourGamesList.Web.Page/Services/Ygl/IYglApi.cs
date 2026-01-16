using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;
using YourGamesList.Common.Http;
using YourGamesList.Common.Refit;
using YourGamesList.Contracts.Dto;
using YourGamesList.Contracts.Requests.Games;
using YourGamesList.Contracts.Requests.Lists;
using YourGamesList.Contracts.Requests.Users;
using YourGamesList.Contracts.Responses.Games;
using YourGamesList.Contracts.Responses.Users;

namespace YourGamesList.Web.Page.Services.Ygl;

public interface IYglApi
{
    public IYglApiAuth Auth { get; }
    public IYglApiUsers Users { get; }
    public IYglApiSearchGames SearchGames { get; }
    public IYglApiLists Lists { get; }
}

public class YglApi : IYglApi
{
    public IYglApiAuth Auth { get; }
    public IYglApiUsers Users { get; }
    public IYglApiSearchGames SearchGames { get; }
    public IYglApiLists Lists { get; }

    public YglApi(IYglApiAuth auth, IYglApiUsers users, IYglApiSearchGames searchGames, IYglApiLists lists)
    {
        Auth = auth;
        Users = users;
        SearchGames = searchGames;
        Lists = lists;
    }
}

public interface IYglApiAuth : IHandlesHttpRefitException
{
    [Post("/users/auth/register")]
    [Headers($"Accept: {ContentTypes.ApplicationJson}", $"Content-Type: {ContentTypes.ApplicationJson}")]
    Task<IApiResponse<Guid>> Register([Body(BodySerializationMethod.Serialized)] AuthUserRegisterRequestBody request);

    [Post("/users/auth/login")]
    [Headers($"Content-Type: {ContentTypes.ApplicationJson}")]
    Task<IApiResponse<AuthLoginResponse>> Login([Body(BodySerializationMethod.Serialized)] AuthUserLoginRequestBody request);

    [Post("/users/auth/delete")]
    [Headers($"Content-Type: {ContentTypes.ApplicationJson}")]
    Task<IApiResponse<Guid>> Delete([Body(BodySerializationMethod.Serialized)] AuthUserDeleteRequestBody request);
}

public interface IYglApiSearchGames : IHandlesHttpRefitException
{
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
}

public interface IYglApiLists : IHandlesHttpRefitException
{
    [Get("/lists/getSelf")]
    [Headers($"Content-Type: {ContentTypes.ApplicationJson}")]
    Task<IApiResponse<List<GamesListDto>>> GetSelfLists(
        [Authorize("Bearer")] string userToken,
        [Query] bool includeGames = false
    );

    [Post("/lists/entries/add")]
    [Headers($"Content-Type: {ContentTypes.ApplicationJson}", $"Content-Type: {ContentTypes.ApplicationJson}")]
    Task<IApiResponse<List<Guid>>> AddListEntries(
        [Authorize("Bearer")] string userToken,
        [Body(BodySerializationMethod.Serialized)]
        AddEntriesToListRequestBody request
    );

    [Patch("/lists/entries/update")]
    [Headers($"Content-Type: {ContentTypes.ApplicationJson}", $"Content-Type: {ContentTypes.ApplicationJson}")]
    Task<IApiResponse<List<Guid>>> UpdateListEntries(
        [Authorize("Bearer")] string userToken,
        [Body(BodySerializationMethod.Serialized)]
        UpdateListEntriesRequestBody request
    );
}

public interface IYglApiUsers : IHandlesHttpRefitException
{
    [Get("/users/getSelf")]
    [Headers($"Accept: {ContentTypes.ApplicationJson}")]
    Task<IApiResponse<UserDto>> GetSelfUser([Authorize("Bearer")] string userToken);

    [Patch("/users/update")]
    [Headers($"Content-Type: {ContentTypes.ApplicationJson}")]
    Task<IApiResponse<Guid>> UpdateUser([Authorize("Bearer")] string userToken,
        [Body(BodySerializationMethod.Serialized)]
        UserUpdateRequestBody request);

    [Get("/users/countries")]
    [Headers($"Accept: {ContentTypes.ApplicationJson}")]
    Task<IApiResponse<string[]>> GetAvailableCountries(
        [Authorize("Bearer")] string userToken
    );
}