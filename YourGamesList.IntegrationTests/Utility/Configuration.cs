using System;
using Microsoft.Extensions.Configuration;

namespace YourGamesList.IntegrationTests.Utility;

public static class Configuration
{
    private static readonly IConfiguration Conf = new ConfigurationBuilder().AddJsonFile("testSettings.json").Build();

    public static string LogFile => ReadOrThrow("LOG_FILE");
    
    public static string YourGamesListBaseUrl => ReadOrThrow("YOUR_GAMES_LIST_BASE_URL");
    
    public static string UserName => ReadOrThrow("TEST_USER_NAME");
    public static string UserPassword => ReadOrThrow("TEST_USER_PASSWORD");


    private static string ReadOrThrow(string varName)
    {
        var val = Conf[varName];
        if (!string.IsNullOrWhiteSpace(val))
        {
            return val;
        }

        throw new Exception($"Could not read '{varName}' environment variable.");
    }
}