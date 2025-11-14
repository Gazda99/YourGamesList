using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YourGamesList.Api.Services.CorrelationId;
using YourGamesList.Api.Services.CorrelationId.Options;
using YourGamesList.Common.Http;
using YourGamesList.Common.Logging;

namespace YourGamesList.Api.Middlewares;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;
    private readonly IOptions<CorrelationIdMiddlewareOptions> _options;
    private readonly ICorrelationIdProvider _correlationIdProvider;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger, IOptions<CorrelationIdMiddlewareOptions> options,
        ICorrelationIdProvider correlationIdProvider)
    {
        _next = next;
        _logger = logger;
        _options = options;
        _correlationIdProvider = correlationIdProvider;
    }

    public async Task Invoke(HttpContext context)
    {
        string corId;
        if (_options.Value.ReadCorrelationIdFromRequestHeader &&
            context.Request.Headers.TryGetValue(HttpHeaders.CorrelationId, out var corIdFromHeader) &&
            _correlationIdProvider.IsValidCorrelationId(corIdFromHeader.ToString()))
        {
            corId = corIdFromHeader.ToString();
        }
        else
        {
            corId = _correlationIdProvider.GetCorrelationId();
        }

        using (_logger.BeginScope(new Dictionary<string, object> { [LogProperties.CorrelationId] = corId }))
        {
            context.Response.OnStarting(state =>
            {
                var httpContext = (HttpContext) state;
                httpContext.Response.Headers.Append(HttpHeaders.CorrelationId, corId);

                return Task.CompletedTask;
            }, context);

            await _next(context);
        }
    }
}