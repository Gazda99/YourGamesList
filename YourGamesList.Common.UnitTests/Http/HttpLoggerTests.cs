using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using YourGamesList.Common.Http;
using YourGamesList.TestsUtils;

namespace YourGamesList.Common.UnitTests.Http;

[TestFixture]
public class HttpLoggerTests
{
    private ILogger<HttpLogger> _logger;
    private HttpLogger _httpLogger;

    [SetUp]
    public void SetUp()
    {
        _logger = Substitute.For<ILogger<HttpLogger>>();
        _httpLogger = new HttpLogger(_logger);
    }

    [Test]
    public void LogRequestStart_LogsInformationMessageAndCreatesScopes()
    {
        // Arrange
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.yourgameslist.com/v1/games?page=1");

        // Act
        var result = _httpLogger.LogRequestStart(request);

        // Assert
        Assert.That(result, Is.Null);

        _logger.ReceivedLog(LogLevel.Information, "Sending 'GET' to 'https://api.yourgameslist.com/v1/games?page=1'");
        _logger.ReceivedBeginScope(new Dictionary<string, object>
        {
            ["Request.Method"] = HttpMethod.Get,
            ["Request.Host"] = "https://api.yourgameslist.com",
            ["Request.Path"] = "/v1/games?page=1"
        });
    }

    [Test]
    public void LogRequestStart_WithCustomHeaders_LogsHeaderScopes()
    {
        // Arrange
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.yourgameslist.com/v1/games");
        request.Headers.Add("X-Correlation-ID", "12345");

        _httpLogger.Options = new HttpLoggerConfiguration
        {
            CustomRequestHeadersToLog = new Dictionary<string, string>
            {
                ["X-Correlation-ID"] = "CorrelationId"
            }
        };

        // Act
        _httpLogger.LogRequestStart(request);

        // Assert
        _logger.ReceivedBeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = "12345"
        });
    }

    [Test]
    public void LogRequestStop_LogsInformationMessageAndCreatesScopes()
    {
        // Arrange
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.yourgameslist.com/v1/games");
        using var response = new HttpResponseMessage(HttpStatusCode.OK);
        var elapsed = TimeSpan.FromMilliseconds(150.47);

        // Act
        _httpLogger.LogRequestStop(null, request, response, elapsed);

        // Assert
        _logger.ReceivedLog(LogLevel.Information, "Received '200 OK' after 150.5ms");
        _logger.ReceivedBeginScope(new Dictionary<string, object>
        {
            ["Response.StatusCodeInt"] = 200,
            ["Response.StatusCodeString"] = HttpStatusCode.OK,
            ["Response.ElapsedMilliseconds"] = "150.5"
        });
    }

    [Test]
    public void LogRequestStop_WithCustomResponseHeaders_LogsHeaderScopes()
    {
        // Arrange
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.yourgameslist.com/v1/games");
        using var response = new HttpResponseMessage(HttpStatusCode.OK);
        response.Headers.Add("X-Trace-ID", "99999");

        _httpLogger.Options = new HttpLoggerConfiguration
        {
            CustomResponseHeadersToLog = new Dictionary<string, string>
            {
                ["X-Trace-ID"] = "TraceId"
            }
        };

        // Act
        _httpLogger.LogRequestStop(null, request, response, TimeSpan.FromMilliseconds(50));

        // Assert
        _logger.ReceivedBeginScope(new Dictionary<string, object>
        {
            ["TraceId"] = "99999"
        });
    }

    [Test]
    public void LogRequestFailed_LogsWarningMessageWithExceptionAndScopes()
    {
        // Arrange
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.yourgameslist.com/v1/games");
        var exception = new HttpRequestException("Timeout error");
        var elapsed = TimeSpan.FromMilliseconds(500.12);

        // Act
        _httpLogger.LogRequestFailed(null, request, null, exception, elapsed);

        // Assert
        _logger.ReceivedLog(LogLevel.Warning, "Request 'GET' towards 'https://api.yourgameslist.com/v1/games' failed after 500.1ms", (ex) => ex == exception);
        _logger.ReceivedBeginScope(new Dictionary<string, object>
        {
            ["Request.Method"] = HttpMethod.Get,
            ["Request.Host"] = "https://api.yourgameslist.com",
            ["Request.Path"] = "/v1/games",
            ["Response.ElapsedMilliseconds"] = "500.1"
        });
    }
}