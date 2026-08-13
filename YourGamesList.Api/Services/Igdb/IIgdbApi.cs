using System.Threading.Tasks;
using Refit;
using YourGamesList.Common.Http;
using YourGamesList.Common.Refit;

namespace YourGamesList.Api.Services.Igdb;

public interface IIgdbApi : IHandlesHttpRefitException
{
    [Post("/{endpointName}")]
    [Headers($"Accept: {ContentTypes.ApplicationJson}", $"Content-Type: {ContentTypes.ApplicationJson}")]
    Task<IApiResponse<TResponse>> Endpoint<TResponse>(
        [AliasAs("endpointName")] string endpointName,
        [Authorize(scheme: "Bearer")] string accessToken,
        [Header("Client-ID")] string clientId,
        [Body] string query
    );
}