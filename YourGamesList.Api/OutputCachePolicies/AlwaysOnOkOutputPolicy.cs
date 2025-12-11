using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Logging;

namespace YourGamesList.Api.OutputCachePolicies;

/// <summary>
/// Serves cached responses only when the response status code is 200 OK. Otherwise, it bypasses the cache.
/// </summary>
// TODO: unit test
public class AlwaysOnOkOutputPolicy : IOutputCachePolicy
{
    private readonly TimeSpan _cacheTtl = TimeSpan.FromHours(1);
    private readonly ILogger<AlwaysOnOkOutputPolicy> _logger;

    public AlwaysOnOkOutputPolicy(ILogger<AlwaysOnOkOutputPolicy> logger)
    {
        _logger = logger;
    }

    //Before actual request processing
    public ValueTask CacheRequestAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        // Always enable caching for requests. We will decide later whether to store or serve from cache, based on the response.
        context.EnableOutputCaching = true;
        context.AllowCacheLookup = true;
        context.AllowCacheStorage = true;
        context.AllowLocking = true;

        context.ResponseExpirationTimeSpan = _cacheTtl;

        return ValueTask.CompletedTask;
    }

    //Should serve?
    public ValueTask ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        _logger.LogInformation($"Serving from cache for request to '{context.HttpContext.Request.Path.Value}'. ");
        return ValueTask.CompletedTask;
    }

    //Should store?
    public ValueTask ServeResponseAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        if (context.HttpContext.Response.StatusCode == StatusCodes.Status200OK)
        {
            _logger.LogInformation($"Response status code is 200 OK. Allowing cache storage. Will store cache for '{_cacheTtl}'");
            context.AllowCacheStorage = true;
        }
        else
        {
            _logger.LogInformation("Response status code is not 200 OK. Disabling cache storage.");
            context.AllowCacheStorage = false;
        }

        return ValueTask.CompletedTask;
    }
}