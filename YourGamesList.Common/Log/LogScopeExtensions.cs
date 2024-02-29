using Microsoft.Extensions.Logging;
using YourGamesList.Common.Http;

namespace YourGamesList.Common.Log;

public static class LogScopeExtensions
{
    public static IDisposable? With<T>(this ILogger<T> logger, string prop, string value)
    {
        return logger.BeginScope(
            new Dictionary<string, object> { [prop] = value });
    }

    public static IDisposable? WithCorrelationId<T>(this ILogger<T> logger)
    {
        return logger.BeginScope(
            new Dictionary<string, object> { [HeaderDefs.HeaderCorrelationIdName] = CreateCorrelationId() });
    }

    public static IDisposable? WithCorrelationId<T>(this ILogger<T> logger, string? corId)
    {
        if (string.IsNullOrEmpty(corId))
            return logger.WithCorrelationId();

        return logger.BeginScope(
            new Dictionary<string, object> { [HeaderDefs.HeaderCorrelationIdName] = corId });
    }

    public static string CreateCorrelationId()
    {
        return Guid.NewGuid().ToString("N");
    }
}