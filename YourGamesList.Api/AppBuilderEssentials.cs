using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using YourGamesList.Api.Options;
using YourGamesList.Common.Http;
using YourGamesList.Common.Options;

namespace YourGamesList.Api;

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

        builder.Services.AddHttpClient("HltbHttpClient", (serviceProvider, httpClient) =>
        {
            var httpClientOptions =
                serviceProvider.GetRequiredService<IOptions<HltbHttpClientOptions>>().Value;

            httpClient.BaseAddress = new Uri(httpClientOptions.BaseAddress);
        }).ConfigureLogging();

        return builder;
    }
}