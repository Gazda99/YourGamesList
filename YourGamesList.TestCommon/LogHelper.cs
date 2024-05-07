using System;

namespace YourGamesList.TestCommon;

public static class LogHelper
{
    public const string CorrelationIdPropertyName = "CorrelationId";

    public static string CreateCorrelationId()
    {
        return Guid.NewGuid().ToString("D");
    }
}