using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace YourGamesList.Common.Refit;

public static class RefitExtensions
{
    public static async Task<CombinedResult<T, HttpFailureReason>> TryRefit<T>(
        this IHandlesHttpRefitException refitApi,
        Func<Task<T>> refitCall,
        ILogger? logger = null,
        string? destinationServiceName = null
    ) where T : notnull
    {
        try
        {
            var result = await refitCall();
            return CombinedResult<T, HttpFailureReason>.Success(result);
        }
        catch (Exception ex)
        {
            var enumValue = ex switch
            {
                TaskCanceledException => HttpFailureReason.Timeout,
                HttpRequestException => HttpFailureReason.Network,
                _ => HttpFailureReason.General,
            };
            
            logger?.LogWarning(ex, $"Http related exception occured while making http request with refit to '{destinationServiceName ?? "N/A"}', results in network error '{enumValue.ToString()}'");

            return CombinedResult<T, HttpFailureReason>.Failure(enumValue);
        }
    }
}