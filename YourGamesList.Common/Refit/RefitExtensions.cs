using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Refit;

namespace YourGamesList.Common.Refit;

public static class RefitExtensions
{
    /// <summary>
    /// IApiResponse&lt;T&gt; overload that handles Refit JSON parsing exceptions
    /// </summary>
    public static async Task<CombinedResult<IApiResponse<T>, RefitFailureReason>> TryRefit<T>(
        this IHandlesRefitFailure refitApi,
        Func<Task<IApiResponse<T>>> refitCall,
        ILogger? logger = null,
        string? destinationServiceName = null
    ) where T : notnull
    {
        try
        {
            var result = await refitCall();

            if (result.Error is ApiException { InnerException: System.Text.Json.JsonException jsonEx })
            {
                const RefitFailureReason reason = RefitFailureReason.Deserialization;
                LogWarning(logger, destinationServiceName, reason, jsonEx, true);
                return CombinedResult<IApiResponse<T>, RefitFailureReason>.Failure(reason);
            }

            return CombinedResult<IApiResponse<T>, RefitFailureReason>.Success(result);
        }
        catch (Exception ex)
        {
            var enumValue = ex switch
            {
                TaskCanceledException => RefitFailureReason.Timeout,
                HttpRequestException => RefitFailureReason.Network,
                _ => RefitFailureReason.General,
            };

            LogWarning(logger, destinationServiceName, enumValue, ex, false);

            return CombinedResult<IApiResponse<T>, RefitFailureReason>.Failure(enumValue);
        }
    }

    private static void LogWarning(ILogger? logger, string? destinationServiceName, RefitFailureReason reason, Exception? ex = null, bool exceptionContainsSensitiveData = true)
    {
        var destLog = destinationServiceName ?? "N/A";
        var isDebugEnabled = logger?.IsEnabled(LogLevel.Debug) ?? false;

        // If exception contains sensitive data, log it only if logging debug mode is on 
        if (ex != null && exceptionContainsSensitiveData && isDebugEnabled)
        {
            logger?.LogWarning(ex, "Exception occurred while making HTTP request with Refit to {DestinationServiceName}, resulting in error {RefitFailureReason}", destLog, reason);
        }
        else
        {
            logger?.LogWarning("Exception occurred while making HTTP request with Refit to {DestinationServiceName}, resulting in error {RefitFailureReason}", destLog, reason);
        }
    }
}