using Lib.ServerTiming;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace YourGamesList.Common.Middleware;

public class ServerTimingMiddleware
{
    private const string ServerTimingMetric = "total";

    private readonly RequestDelegate _next;

    public ServerTimingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var serverTiming = context.RequestServices.GetRequiredService<IServerTiming>();
        var timer = serverTiming.TimeAction(ServerTimingMetric);

        context.Response.OnStarting(() =>
        {
            timer.Dispose();
            return Task.CompletedTask;
        });

        await _next(context);
    }
}