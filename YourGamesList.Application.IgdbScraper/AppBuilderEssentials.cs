using Microsoft.Extensions.Options;
using YourGamesList.Common.Http;
using YourGamesList.Services.Igdb.Options;
using YourGamesList.Services.Twitch.Options;

namespace YourGamesList.Application.IgdbScraper;

public static class AppBuilderEssentials
{
    public static WebApplicationBuilder AddConfigFiles(this WebApplicationBuilder builder)
    {
        const string envVariableName = "ENV";
        var env = Environment.GetEnvironmentVariable(envVariableName);

        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env}.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

#if DEBUG
        builder.Configuration.AddJsonFile("serilogsettings.debug.json", optional: false, reloadOnChange: true);
#else
        builder.Configuration.AddJsonFile($"serilogsettings.{env}.json", optional: false, reloadOnChange: true);
#endif

        return builder;
    }

    public static WebApplicationBuilder AddHttpClients(this WebApplicationBuilder builder)
    {
        builder.Services.AddHttpClient("TwitchAuthHttpClient", (serviceProvider, httpClient) =>
        {
            var httpClientOptions =
                serviceProvider.GetRequiredService<IOptions<TwitchAuthHttpClientOptions>>().Value;

            httpClient.BaseAddress = new Uri(httpClientOptions.BaseAddress);
        }).ConfigureLogging();

        builder.Services.AddHttpClient("IgdbHttpClient", (serviceProvider, httpClient) =>
        {
            var httpClientOptions =
                serviceProvider.GetRequiredService<IOptions<IgdbHttpClientOptions>>().Value;

            httpClient.BaseAddress = new Uri(httpClientOptions.BaseAddress);
        }).ConfigureLogging();


        return builder;
    }
}