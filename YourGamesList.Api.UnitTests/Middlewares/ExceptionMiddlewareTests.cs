using System;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using YourGamesList.Api.Middlewares;
using YourGamesList.TestsUtils;

namespace YourGamesList.Api.UnitTests.Middlewares;

public class ExceptionMiddlewareTests
{
    private IFixture _fixture;
    private ILogger<ExceptionMiddleware> _logger;

    [SetUp]
    public void Setup()
    {
        _fixture = new  Fixture();
        _logger = Substitute.For<ILogger<ExceptionMiddleware>>();
    }

    [Test]
    public async Task Invoke_OnException_HandlesCorrectly()
    {
        //ARANGE
        var ex = _fixture.Create<Exception>();
        RequestDelegate next = _ => throw ex;

        var exceptionMiddleware = new ExceptionMiddleware(next, _logger);
        var context = new DefaultHttpContext();

        //ACT
        await exceptionMiddleware.Invoke(context);

        //ASSERT
        Assert.That(context.Response.StatusCode, Is.EqualTo((int) HttpStatusCode.InternalServerError));
        _logger.ReceivedLog(LogLevel.Error, $"Unhandled exception during executing {context.Request.Path.Value}");
    }

    [Test]
    public async Task Invoke_OnUnauthorizedAccessException_HandlesCorrectly()
    {
        //ARANGE
        var ex = _fixture.Create<UnauthorizedAccessException>();

        RequestDelegate next = _ => throw ex;

        var exceptionMiddleware = new ExceptionMiddleware(next, _logger);
        var context = new DefaultHttpContext();

        //ACT
        await exceptionMiddleware.Invoke(context);

        //ASSERT
        Assert.That(context.Response.StatusCode, Is.EqualTo((int) HttpStatusCode.Unauthorized));
        _logger.ReceivedLog(LogLevel.Information, $"Unauthorized access exception during executing {context.Request.Path.Value}");
    }
}