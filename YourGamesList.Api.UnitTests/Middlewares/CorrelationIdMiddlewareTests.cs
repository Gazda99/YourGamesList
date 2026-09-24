using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using NSubstitute;
using YourGamesList.Api.Middlewares;
using YourGamesList.Api.Services.CorrelationId;
using YourGamesList.Common.Http;
using YourGamesList.Common.Logging;
using YourGamesList.TestsUtils;

namespace YourGamesList.Api.UnitTests.Middlewares;

public class CorrelationIdMiddlewareTests
{
    private IFixture _fixture;
    private ILogger<CorrelationIdMiddleware> _logger;
    private ICorrelationIdProvider _correlationIdProvider;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _logger = Substitute.For<ILogger<CorrelationIdMiddleware>>();
        _correlationIdProvider = Substitute.For<ICorrelationIdProvider>();
    }

    [Test]
    public async Task CorrelationIdMiddleware_CreateNewCorId_AddCorrelationIdToResponseHeader()
    {
        //GIVEN
        var corId = _fixture.Create<string>();
        _correlationIdProvider.GetCorrelationId().Returns(corId);

        HttpContext ctx = new DefaultHttpContext();
        RequestDelegate next = async (innerHttpContext) => { await Task.CompletedTask; };

        var feature = Substitute.For<IHttpResponseFeature>();
        var responseHeaderDict = new HeaderDictionary();
        feature.Headers.Returns(responseHeaderDict);
        feature.OnStarting(Arg.Do<Func<object, Task>>(f => f(ctx)), Arg.Any<object>());
        ctx.Features.Set<IHttpResponseFeature>(feature);

        var middleware = new CorrelationIdMiddleware(next, _logger, _correlationIdProvider);

        //WHEN
        await middleware.Invoke(ctx);

        //THEN
        var isCorIdPresent = ctx.Response.Headers.TryGetValue(YglHttpHeaders.CorrelationId, out var actualCorId);
        Assert.That(isCorIdPresent, Is.True);
        Assert.That(actualCorId.ToString(), Is.EquivalentTo(corId));
        _logger.ReceivedBeginScope(new Dictionary<string, object>()
        {
            { LogProperties.CorrelationId, corId }
        });
    }

    [Test]
    public async Task CorrelationIdMiddleware_ReadFromRequestHeader_AddCorrelationIdToResponseHeader()
    {
        //GIVEN
        var corId = _fixture.Create<string>();
        _correlationIdProvider.IsValidCorrelationId(corId).Returns(true);

        HttpContext ctx = new DefaultHttpContext();
        ctx.Request.Headers.Append(YglHttpHeaders.CorrelationId, corId);

        RequestDelegate next = async (innerHttpContext) => { await Task.CompletedTask; };

        var feature = Substitute.For<IHttpResponseFeature>();
        var responseHeaderDict = new HeaderDictionary();
        feature.Headers.Returns(responseHeaderDict);
        feature.OnStarting(Arg.Do<Func<object, Task>>(f => f(ctx)), Arg.Any<object>());
        ctx.Features.Set<IHttpResponseFeature>(feature);

        var middleware = new CorrelationIdMiddleware(next, _logger, _correlationIdProvider);

        //WHEN
        await middleware.Invoke(ctx);

        //THEN
        var isCorIdPresent = ctx.Response.Headers.TryGetValue(YglHttpHeaders.CorrelationId, out var actualCorId);
        Assert.That(isCorIdPresent, Is.True);
        Assert.That(actualCorId.ToString(), Is.EquivalentTo(corId));
        _logger.ReceivedBeginScope(new Dictionary<string, object>()
        {
            { LogProperties.CorrelationId, corId }
        });
    }
}