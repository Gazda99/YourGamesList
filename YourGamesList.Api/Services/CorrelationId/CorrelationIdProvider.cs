using System;

namespace YourGamesList.Api.Services.CorrelationId;

public interface ICorrelationIdProvider
{
    bool IsValidCorrelationId(string correlationId);
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