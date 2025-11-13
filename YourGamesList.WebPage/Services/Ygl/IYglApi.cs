using System.Threading.Tasks;
using Refit;
using YourGamesList.Common.Http;
using YourGamesList.Common.Refit;
using YourGamesList.WebPage.Services.Ygl.Model.Requests;
using YourGamesList.WebPage.Services.Ygl.Model.Responses;

namespace YourGamesList.WebPage.Services.Ygl;

public interface IYglAuthApi : IHandlesHttpRefitException
{
    [Post("/auth/login")]
    [Headers($"Accept: {ContentTypes.ApplicationJson}", $"Content-Type: {ContentTypes.ApplicationJson}")]
    Task<IApiResponse<LoginResponse>> Login([Body(BodySerializationMethod.Serialized)] LoginRequest request);
}