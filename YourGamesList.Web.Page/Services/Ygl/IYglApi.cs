using System.Threading.Tasks;
using Refit;
using YourGamesList.Common.Http;
using YourGamesList.Common.Refit;
using YourGamesList.Web.Page.Services.Ygl.Model.Requests;
using YourGamesList.Web.Page.Services.Ygl.Model.Responses;

namespace YourGamesList.Web.Page.Services.Ygl;

public interface IYglAuthApi : IHandlesHttpRefitException
{
    [Post("/auth/login")]
    [Headers($"Accept: {ContentTypes.ApplicationJson}", $"Content-Type: {ContentTypes.ApplicationJson}")]
    Task<IApiResponse<LoginResponse>> Login([Body(BodySerializationMethod.Serialized)] LoginRequest request);
}