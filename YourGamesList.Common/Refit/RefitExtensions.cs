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
        ILogger? logger = null) where T : notnull
    {
        try
        {
            var result = await refitCall();
            return CombinedResult<T, HttpFailureReason>.Success(result);
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Http related exception occured while making http request with refit");

            var enumValue = ex switch
            {
                TaskCanceledException => HttpFailureReason.Timeout,
                HttpRequestException => HttpFailureReason.Network,
                _ => HttpFailureReason.General,
            };

            return CombinedResult<T, HttpFailureReason>.Failure(enumValue);
        }
    }
}