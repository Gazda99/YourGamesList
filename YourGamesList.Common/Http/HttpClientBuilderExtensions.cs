using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace YourGamesList.Common.Http;

[ExcludeFromCodeCoverage]
public static class HttpClientBuilderExtensions
{
    public static IHttpClientBuilder ConfigureLogging(this IHttpClientBuilder builder)
    {
        builder.Services.TryAddScoped<HttpLogger>();
        return builder.RemoveAllLoggers().AddLogger<HttpLogger>(true);
    }
}