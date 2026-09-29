using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NSubstitute;
using Refit;

namespace YourGamesList.TestsUtils;

public static class RefitHelper
{
    public static async Task<ApiException> CreateApiException(
        HttpRequestMessage message,
        HttpMethod httpMethod,
        HttpStatusCode statusCode,
        Exception? innerException = null)
    {
        using var response = new HttpResponseMessage(statusCode);
        return await ApiException.Create(message, httpMethod, response, new RefitSettings(), innerException);
    }

    public static IApiResponse<T> ApiResponseSubstitute<T>(HttpStatusCode statusCode, T content)
    {
        var apiResponse = Substitute.For<IApiResponse<T>>();
        apiResponse.StatusCode.Returns(statusCode);
        apiResponse.Content.Returns(content);
        return apiResponse;
    }
}