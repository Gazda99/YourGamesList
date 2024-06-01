using Lib.ServerTiming;
using Microsoft.AspNetCore.Http;
using YourGamesList.Common.Middlewares;

namespace YourGamesList.Common.UnitTests.Middlewares;

public class ServerTimingsMiddlewareTests
{
    [Test]
    public async Task ServerTimingsMiddleware_Should_BeCalled()
    {
        //GIVEN
        const string serverTimingTotal = "total";
        HttpContext ctx = new DefaultHttpContext();
        RequestDelegate next = async (innerHttpContext) => { await Task.CompletedTask; };
        var serviceProvider = Substitute.For<IServiceProvider>();
        var serverTiming = Substitute.For<IServerTiming>();
        serviceProvider.GetService(typeof(IServerTiming)).Returns(serverTiming);
        ctx.RequestServices = serviceProvider;
        var middleware = new ServerTimingsMiddleware(next);

        //WHEN
        await middleware.Invoke(ctx);

        //THEN
        serviceProvider.Received(1).GetService(typeof(IServerTiming));
        serverTiming.Received(1).TimeAction(serverTimingTotal);
    }
}