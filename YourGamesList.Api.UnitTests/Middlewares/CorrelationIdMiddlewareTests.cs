using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using YourGamesList.Api.Middlewares;
using YourGamesList.Api.Services.CorrelationId;
using YourGamesList.Api.Services.CorrelationId.Options;
using YourGamesList.Common.Http;
using YourGamesList.Common.Logging;

namespace YourGamesList.Api.UnitTests.Middlewares;

public class CorrelationIdMiddlewareTests
{
    private IFixture _fixture;
    private ILogger<CorrelationIdMiddleware> _logger;
    private IOptions<CorrelationIdMiddlewareOptions> _options;
    private ICorrelationIdProvider _correlationIdProvider;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _logger = Substitute.For<ILogger<CorrelationIdMiddleware>>();
        _options = Substitute.For<IOptions<CorrelationIdMiddlewareOptions>>();
        _correlationIdProvider = Substitute.For<ICorrelationIdProvider>();
    }

    [Test]
    public async Task CorrelationIdMiddleware_CreateNewCorId_AddCorrelationIdToResponseHeader()
    {
        //GIVEN
        var corId = _fixture.Create<string>();
        _correlationIdProvider.GetCorrelationId().Returns(corId);

        var options = _fixture.Build<CorrelationIdMiddlewareOptions>()
            .With(x => x.ReadCorrelationIdFromRequestHeader, false)
            .WithAutoProperties()
            .Create();
        _options.Value.Returns(options);

        HttpContext ctx = new DefaultHttpContext();
        RequestDelegate next = async (innerHttpContext) => { await Task.CompletedTask; };

        var feature = Substitute.For<IHttpResponseFeature>();
        var responseHeaderDict = new HeaderDictionary();
        feature.Headers.Returns(responseHeaderDict);
        feature.OnStarting(Arg.Do<Func<object, Task>>(f => f(ctx)), Arg.Any<object>());
        ctx.Features.Set<IHttpResponseFeature>(feature);

        var middleware = new CorrelationIdMiddleware(next, _logger, _options, _correlationIdProvider);

        //WHEN
        await middleware.Invoke(ctx);

        //THEN
        var isCorIdPresent = ctx.Response.Headers.TryGetValue(YglHttpHeaders.CorrelationId, out var actualCorId);
        Assert.That(isCorIdPresent, Is.True);
        Assert.That(actualCorId.ToString(), Is.EquivalentTo(corId));
        _logger.Received(1).BeginScope(Arg.Is<Dictionary<string, object>>(x => x.ContainsKey(LogProperties.CorrelationId) && x.ContainsValue(corId)));
    }

    [Test]
    public async Task CorrelationIdMiddleware_ReadFromRequestHeader_AddCorrelationIdToResponseHeader()
    {
        //GIVEN
        var corId = _fixture.Create<string>();
        _correlationIdProvider.IsValidCorrelationId(corId).Returns(true);

        var options = _fixture.Build<CorrelationIdMiddlewareOptions>()
            .With(x => x.ReadCorrelationIdFromRequestHeader, true)
            .WithAutoProperties()
            .Create();
        _options.Value.Returns(options);

        HttpContext ctx = new DefaultHttpContext();
        ctx.Request.Headers.Append(YglHttpHeaders.CorrelationId, corId);

        RequestDelegate next = async (innerHttpContext) => { await Task.CompletedTask; };

        var feature = Substitute.For<IHttpResponseFeature>();
        var responseHeaderDict = new HeaderDictionary();
        feature.Headers.Returns(responseHeaderDict);
        feature.OnStarting(Arg.Do<Func<object, Task>>(f => f(ctx)), Arg.Any<object>());
        ctx.Features.Set<IHttpResponseFeature>(feature);

        var middleware = new CorrelationIdMiddleware(next, _logger, _options, _correlationIdProvider);

        //WHEN
        await middleware.Invoke(ctx);

        //THEN
        var isCorIdPresent = ctx.Response.Headers.TryGetValue(YglHttpHeaders.CorrelationId, out var actualCorId);
        Assert.That(isCorIdPresent, Is.True);
        Assert.That(actualCorId.ToString(), Is.EquivalentTo(corId));
        _logger.Received(1).BeginScope(Arg.Is<Dictionary<string, object>>(x => x.ContainsKey(LogProperties.CorrelationId) && x.ContainsValue(corId)));
    }
}