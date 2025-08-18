using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using AutoFixture;
using Microsoft.Extensions.Logging;
using NSubstitute;
using YourGamesList.Common.Http;
using YourGamesList.TestsUtils;

namespace YourGamesList.Common.UnitTests.Http;

public class HttpLoggerTests
{
    private IFixture _fixture;
    private ILogger<HttpLogger> _logger;
    private HttpLogger _httpLogger;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _logger = Substitute.For<ILogger<HttpLogger>>();
        _httpLogger = new HttpLogger(_logger);
    }

    [Test]
    public void LogRequestStart_AddsRequiredScopesAndLogsProperMessage()
    {
        //ARRANGE
        var requestUri = _fixture.Create<Uri>();
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        //ACT
        _httpLogger.LogRequestStart(request);

        //ASSERT
        _logger.ReceivedBeginScope(new Dictionary<string, object>
        {
            ["Request.Method"] = request.Method,
            ["Request.Host"] = request.RequestUri?.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped) ?? "N/A",
            ["Request.Path"] = request.RequestUri!.PathAndQuery
        });
        _logger.ReceivedLog(LogLevel.Information,
            $"Sending '{request.Method}' to '{request.RequestUri?.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped)}{request.RequestUri!.PathAndQuery}'");
    }

    [Test]
    public void LogRequestStop_AddsRequiredScopesAndLogsProperMessage()
    {
        //ARRANGE
        var requestUri = _fixture.Create<Uri>();
        var context = _fixture.Create<object?>();
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        var elapsed = _fixture.Create<TimeSpan>();

        //ACT
        _httpLogger.LogRequestStop(context, request, response, elapsed);

        //ASSERT
        _logger.ReceivedBeginScope(new Dictionary<string, object>
        {
            ["Response.StatusCodeInt"] = (int) response.StatusCode,
            ["Response.StatusCodeString"] = response.StatusCode,
            ["Response.ElapsedMilliseconds"] = elapsed.TotalMilliseconds.ToString("F1")
        });
        _logger.ReceivedLog(LogLevel.Information, $"Received '{(int) response.StatusCode} {response.StatusCode}' after {elapsed.TotalMilliseconds:F1}ms");
    }

    [Test]
    public void LogRequestFailed_AddsRequiredScopesAndLogsProperMessage()
    {
        //ARRANGE
        var requestUri = _fixture.Create<Uri>();
        var context = _fixture.Create<object?>();
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        var elapsed = _fixture.Create<TimeSpan>();
        var exception = _fixture.Create<Exception>();

        //ACT
        _httpLogger.LogRequestFailed(context, request, response, exception, elapsed);

        //ASSERT
        _logger.ReceivedBeginScope(new Dictionary<string, object>
        {
            ["Request.Method"] = request.Method,
            ["Request.Host"] = request.RequestUri?.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped) ?? "N/A",
            ["Request.Path"] = request.RequestUri!.PathAndQuery,
            ["Response.ElapsedMilliseconds"] = elapsed.TotalMilliseconds.ToString("F1")
        });
        _logger.ReceivedLog(LogLevel.Warning,
            $"Request '{request.Method}' towards '{request.RequestUri?.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped)}{request.RequestUri!.PathAndQuery}' failed after {elapsed.TotalMilliseconds:F1}ms");
    }
}