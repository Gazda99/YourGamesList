using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using YourGamesList.Common.Middlewares;

namespace YourGamesList.Common.UnitTests.Middlewares;

public class CorrelationIdMiddlewareTests
{
    [Test]
    public async Task CorrelationIdMiddleware_Should_AddCorrelationIdToResponseHeader()
    {
        //GIVEN
        var logger = Substitute.For<ILogger<CorrelationIdMiddleware>>();
        HttpContext ctx = new DefaultHttpContext();
        var headerDict = new HeaderDictionary();
        RequestDelegate next = async (innerHttpContext) => { await Task.CompletedTask; };

        var feature = Substitute.For<IHttpResponseFeature>();
        feature.Headers.Returns(headerDict);
        feature.OnStarting(Arg.Do<Func<object, Task>>(f => f(ctx)), Arg.Any<object>());
        ctx.Features.Set<IHttpResponseFeature>(feature);

        var middleware = new CorrelationIdMiddleware(next, logger);

        //WHEN
        await middleware.Invoke(ctx);

        //THEN
        ctx.Response.Headers.TryGetValue(HttpHelper.HeaderCorrelationIdName, out var corIdHeaderValue).Should()
            .BeTrue();
        Guid.TryParse(corIdHeaderValue, out _).Should().BeTrue();
        logger.Received(0).BeginScope(Arg.Is<Dictionary<string, object>>(x =>
            x.ContainsKey(LogHelper.CorrelationIdPropertyName) && x.ContainsValue(corIdHeaderValue)));
    }
}