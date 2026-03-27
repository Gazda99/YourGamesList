using System.Net;
using NSubstitute;
using Refit;

namespace YourGamesList.TestsUtils;

public static class RefitHelper
{
    public static IApiResponse<T> ApiResponseSubstitute<T>(HttpStatusCode statusCode, T content)
    {
        var apiResponse = Substitute.For<IApiResponse<T>>();
        apiResponse.StatusCode.Returns(statusCode);
        apiResponse.Content.Returns(content);
        return apiResponse;
    }
}