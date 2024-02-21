using Microsoft.AspNetCore.Http;

namespace YourGamesList.Common.Http;

public static class HeadersExtensions
{
    public static string GetCorrelationId(this IHeaderDictionary headers)
    {
        var correlationId = headers.FirstOrDefault(i =>
            i.Key.Equals(HeaderDefs.HeaderCorrelationIdName, StringComparison.InvariantCultureIgnoreCase)).Value;

        return correlationId.ToString();
    }

    public static IHeaderDictionary AddCorrelationId(this IHeaderDictionary headers, string? corId)
    {
        if (string.IsNullOrEmpty(corId))
            corId = "N/A";

        headers.TryAdd(HeaderDefs.HeaderCorrelationIdName, corId);
        return headers;
    }
}