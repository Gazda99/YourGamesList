using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http.Resilience;
using Polly;

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

    /// <summary>
    /// Adds a resilience pipeline using the <see cref="HttpResilienceOptions"/> exposed by the client's own options class
    /// (<typeparamref name="TClientOptions"/>), which must be registered and validated separately.
    /// When the client has no resilience options configured, <see cref="HttpResilienceOptions"/> defaults are used.
    /// Currently only applies a total request timeout; each call is still a single attempt (no retries).
    /// </summary>
    public static IHttpClientBuilder ConfigureResilience<TClientOptions>(
        this IHttpClientBuilder builder,
        Func<TClientOptions, HttpResilienceOptions?> resilienceOptionsSelector
    ) where TClientOptions : class
    {
        builder.AddResilienceHandler(ResiliencePipelineName, (pipeline, context) =>
        {
            context.EnableReloads<TClientOptions>();
            var options = resilienceOptionsSelector(context.GetOptions<TClientOptions>()) ?? new HttpResilienceOptions();

            pipeline.AddTimeout(options.TotalRequestTimeout ?? new HttpTimeoutStrategyOptions());
        });

        return builder;
    }

    private const string ResiliencePipelineName = "ygl-resilience";
}