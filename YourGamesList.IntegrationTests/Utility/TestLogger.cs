using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Refit;

namespace YourGamesList.IntegrationTests.Utility;

public static class TestLogger
{
    private static readonly string LogFile = Configuration.LogFile;

    static TestLogger()
    {
        //Clear file each time we run tests
        File.WriteAllText(LogFile, string.Empty);
    }

    public static async Task Log(string msg)
    {
        var formattedMessage = $"[{DateTime.Now:HH:mm:ss.fff}] {msg}";
        await TestContext.Out.WriteLineAsync(formattedMessage);
        await File.AppendAllTextAsync(LogFile, $"{formattedMessage}\n");
    }

    public static async Task LogApiResponse(IApiResponse apiResponse)
    {
        var log =
            $"'{apiResponse.RequestMessage?.Method}' request to '{apiResponse.RequestMessage?.RequestUri}' completed with status code: '{StatusCodeInfo(apiResponse.StatusCode)}'";
        await Log(log);
    }

    public static async Task LogApiResponse<T>(IApiResponse<T> apiResponse)
    {
        var body = apiResponse.Content != null ? apiResponse.Content.ToString() : apiResponse.Error?.Content?.ToString();
        var log =
            $"'{apiResponse.RequestMessage?.Method}' request to '{apiResponse.RequestMessage?.RequestUri}' completed with status code: '{StatusCodeInfo(apiResponse.StatusCode)}'. Body '{body}'";
        await Log(log);
    }

    private static string StatusCodeInfo(HttpStatusCode statusCode)
    {
        return $"{(int) statusCode} {statusCode.ToString()}";
    }
}