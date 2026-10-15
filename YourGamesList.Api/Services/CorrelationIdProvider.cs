using System;

namespace YourGamesList.Api.Services;

public interface ICorrelationIdProvider
{
    /// <summary>
    /// Checks whether a correlation ID is valid.
    /// </summary>
    bool IsValidCorrelationId(string correlationId);

    /// <summary>
    /// Creates a new correlation ID.
    /// </summary>
    string GetCorrelationId();
}

public class CorrelationIdProvider : ICorrelationIdProvider
{
    public bool IsValidCorrelationId(string correlationId)
    {
        return Guid.TryParseExact(correlationId, "N", out _);
    }

    public string GetCorrelationId()
    {
        return Guid.NewGuid().ToString("N");
    }
}