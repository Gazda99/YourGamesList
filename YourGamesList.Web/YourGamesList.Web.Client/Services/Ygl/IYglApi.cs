using System.Threading.Tasks;
using Refit;
using YourGamesList.Common.Http;
using YourGamesList.Common.Refit;
using YourGamesList.Web.Client.Services.Ygl.Model.Requests;

namespace YourGamesList.Web.Client.Services.Ygl;

public interface IYglAuthApi : IHandlesHttpRefitException
{
    [Post("/auth/login")]
    [Headers($"Accept: {ContentTypes.ApplicationJson}", $"Content-Type: {ContentTypes.ApplicationJson}")]
    Task<IApiResponse<string>> Login([Body(BodySerializationMethod.Serialized)] LoginRequest request);
}