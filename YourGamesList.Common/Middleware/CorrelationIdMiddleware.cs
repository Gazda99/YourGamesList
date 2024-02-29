using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using YourGamesList.Common.Http;
using YourGamesList.Common.Log;

namespace YourGamesList.Common.Middleware;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var corId = LogScopeExtensions.CreateCorrelationId();
        using (_logger.WithCorrelationId(corId))
        {
            context.Response.OnStarting(state =>
            {
                var httpContext = (HttpContext) state;
                httpContext.Response.Headers.AddCorrelationId(corId);

                return Task.CompletedTask;
            }, context);
            
            await _next(context);
        }
    }
}