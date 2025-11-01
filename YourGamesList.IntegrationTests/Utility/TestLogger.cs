using System;
using System.IO;
using System.Threading.Tasks;
using Refit;

namespace YourGamesList.IntegrationTests.Utility;

public static class TestLogger
{
    private static readonly string LogFile = Configuration.LogFile;

    public static async Task Log(string msg)
    {
        var formattedMessage = $"[{DateTime.Now:HH:mm:ss.fff}] {msg}";
        await TestContext.Out.WriteLineAsync(formattedMessage);
        await File.AppendAllTextAsync(LogFile, $"{formattedMessage}\n");
    }

    public static async Task LogApiResponse(IApiResponse apiResponse)
    {
        var log = $"'{apiResponse.RequestMessage.Method}' request to '{apiResponse.RequestMessage.RequestUri}' completed with status code: '{(int) apiResponse.StatusCode}'";
        await Log(log);
    }
}