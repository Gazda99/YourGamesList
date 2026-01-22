using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace YourGamesList.Common.Http;

[ExcludeFromCodeCoverage]
public static class HttpClientBuilderExtensions
{
    public static IHttpClientBuilder ConfigureLogging(this IHttpClientBuilder builder, Action<HttpLoggerConfiguration>? configureOptions = null)
    {
        builder.Services.TryAddScoped<HttpLogger>();
        return builder.RemoveAllLoggers().AddLogger(sp =>
        {
            var logger = sp.GetRequiredService<HttpLogger>();

            var options = logger.Options ?? new HttpLoggerConfiguration();

            configureOptions?.Invoke(options);

            logger.Options = options;
            return logger;
        }, true);
    }
}