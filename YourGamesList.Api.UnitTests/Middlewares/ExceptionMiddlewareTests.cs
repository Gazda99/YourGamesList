using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using YourGamesList.Api.Middlewares;
using YourGamesList.TestsUtils;

namespace YourGamesList.Api.UnitTests.Middlewares;

public class ExceptionMiddlewareTests
{
    private ILogger<ExceptionMiddleware> _logger;

    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger<ExceptionMiddleware>>();
    }

    [Test]
    public async Task Invoke_Handle_Exception()
    {
        //ARANGE
        var ex = new Exception();
        RequestDelegate next = _ => throw ex;

        var exceptionMiddleware = new ExceptionMiddleware(next, _logger);
        var context = new DefaultHttpContext();

        //ACT
        await exceptionMiddleware.Invoke(context);

        //ASSERT
        Assert.That(context.Response.StatusCode, Is.EqualTo((int) HttpStatusCode.InternalServerError));
        _logger.ReceivedLog(LogLevel.Error, $"Unhandled exception during executing '{context.Request.Path.Value}'");
    }
}