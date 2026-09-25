using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Services;
using YourGamesList.Common.Http;
using YourGamesList.Common.Logging;

namespace YourGamesList.Api.Middlewares;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;
    private readonly ICorrelationIdProvider _correlationIdProvider;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger, ICorrelationIdProvider correlationIdProvider)
    {
        _next = next;
        _logger = logger;
        _correlationIdProvider = correlationIdProvider;
    }

    public async Task Invoke(HttpContext context)
    {
        string corId;
        if (context.Request.Headers.TryGetValue(YglHttpHeaders.CorrelationId, out var corIdFromHeader) && _correlationIdProvider.IsValidCorrelationId(corIdFromHeader.ToString()))
        {
            corId = corIdFromHeader.ToString();
        }
        else
        {
            corId = _correlationIdProvider.GetCorrelationId();
        }

        var activity = Activity.Current;
        activity?.SetTag("correlation_id", corId);

        using (_logger.BeginScope(new Dictionary<string, object> { [LogProperties.CorrelationId] = corId }))
        {
            context.Response.OnStarting(state =>
            {
                var httpContext = (HttpContext) state;
                httpContext.Response.Headers.Append(YglHttpHeaders.CorrelationId, corId);

                return Task.CompletedTask;
            }, context);

            await _next(context);
        }
    }
}