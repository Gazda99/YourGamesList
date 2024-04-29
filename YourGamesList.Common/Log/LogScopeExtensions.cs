using Microsoft.Extensions.Logging;

namespace YourGamesList.Common.Log;

public static class LogScopeExtensions
{
    private const string CorrelationIdPropertyName = "CorrelationId";

    public static IDisposable? With<T>(this ILogger<T> logger, string prop, string value)
    {
        return logger.BeginScope(
            new Dictionary<string, object> { [prop] = value });
    }

    public static IDisposable? WithCorrelationId<T>(this ILogger<T> logger)
    {
        return logger.BeginScope(
            new Dictionary<string, object> { [CorrelationIdPropertyName] = CreateCorrelationId() });
    }

    public static IDisposable? WithCorrelationId<T>(this ILogger<T> logger, string? corId)
    {
        if (string.IsNullOrEmpty(corId))
            return logger.WithCorrelationId();

        return logger.BeginScope(
            new Dictionary<string, object> { [CorrelationIdPropertyName] = corId });
    }

    public static string CreateCorrelationId()
    {
        return Guid.NewGuid().ToString("D");
    }
}