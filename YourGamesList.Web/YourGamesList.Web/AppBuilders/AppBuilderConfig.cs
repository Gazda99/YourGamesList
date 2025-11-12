using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace YourGamesList.Web.AppBuilders;

public static partial class AppBuilder
{
    private static WebApplicationBuilder AddConfigAndVariables(this WebApplicationBuilder builder)
    {
        const string envVariableName = "ENV";
        var env = Environment.GetEnvironmentVariable(envVariableName);
        env = env?.ToLower();

        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        builder.Configuration
            .AddJsonFile($"appsettings.{env}.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

#if DEBUG
        builder.Configuration.AddJsonFile("serilogsettings.debug.json", optional: false, reloadOnChange: true);
#else
        builder.Configuration.AddJsonFile($"serilogsettings.{env}.json", optional: false, reloadOnChange: true);
#endif

        return builder;
    }
}