using System.Net;
using Microsoft.Extensions.Logging;
using TddXt.AnyRoot;
using TddXt.AnyRoot.Numbers;
using YourGamesList.Common.Http;

namespace YourGamesList.Common.UnitTests.Http;

public class HttpLoggerTests
{
    [Test]
    public void LogRequestStart_Should_LogProperMessageWithScopes()
    {
        //GIVEN
        var logger = Substitute.For<ILogger<HttpLogger>>();
        var request = PrepareRequestMessage();

        var httpLogger = new HttpLogger(logger);

        //WHEN
        httpLogger.LogRequestStart(request);

        //THEN
        logger.Received(1).BeginScope(Arg.Is<Dictionary<string, object>>(x =>
            x.ContainsKey("Request.Method")
            && (HttpMethod) x["Request.Method"] == request.Method
            && x.ContainsKey("Request.Host")
            && (string) x["Request.Host"] ==
            request.RequestUri!.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped)
            && x.ContainsKey("Request.Path")
            && (string) x["Request.Path"] == request.RequestUri!.PathAndQuery
        ));
        logger.Received(1)
            .LogInformation(
                $"Sending '{request.Method}' to '{request.RequestUri?.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped)}{request.RequestUri!.PathAndQuery}'");
    }

    [Test]
    public void LogRequestStop_Should_LogProperMessageWithScopes()
    {
        //GIVEN
        var logger = Substitute.For<ILogger<HttpLogger>>();
        var request = PrepareRequestMessage();
        var response = PrepareResponseMessage();
        var timespan = TimeSpan.FromMilliseconds(Any.Integer());

        var httpLogger = new HttpLogger(logger);

        //WHEN
        httpLogger.LogRequestStop(Any.Object(), request, response, timespan);

        //THEN
        logger.Received(1).BeginScope(Arg.Is<Dictionary<string, object>>(x =>
            x.ContainsKey("Response.StatusCodeInt")
            && (int) x["Response.StatusCodeInt"] == ((int) response.StatusCode)
            && x.ContainsKey("Response.StatusCodeString")
            && (HttpStatusCode) x["Response.StatusCodeString"] == response.StatusCode
            && x.ContainsKey("Response.ElapsedMilliseconds")
            && (string) x["Response.ElapsedMilliseconds"] == timespan.TotalMilliseconds.ToString("F1")
        ));
        logger.Received(1)
            .LogInformation(
                $"Received '{(int) response.StatusCode} {response.StatusCode}' after {timespan.TotalMilliseconds:F1}ms");
    }

    [Test]
    public void LogRequestFailed_Should_LogProperMessageWithScopes()
    {
        //GIVEN
        var logger = Substitute.For<ILogger<HttpLogger>>();
        var request = PrepareRequestMessage();
        var response = PrepareResponseMessage();
        var timespan = TimeSpan.FromMilliseconds(Any.Integer());
        var exception = Any.Instance<Exception>();

        var httpLogger = new HttpLogger(logger);

        //WHEN
        httpLogger.LogRequestFailed(Any.Object(), request, response, exception, timespan);

        //THEN
        logger.Received(1).BeginScope(Arg.Is<Dictionary<string, object>>(x =>
            x.ContainsKey("Request.Method")
            && (HttpMethod) x["Request.Method"] == request.Method
            && x.ContainsKey("Request.Host")
            && (string) x["Request.Host"] ==
            request.RequestUri!.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped)
            && x.ContainsKey("Request.Path")
            && (string) x["Request.Path"] == request.RequestUri!.PathAndQuery
            && x.ContainsKey("Response.ElapsedMilliseconds")
            && (string) x["Response.ElapsedMilliseconds"] == timespan.TotalMilliseconds.ToString("F1")
        ));
        logger.Received(1)
            .LogError(exception,
                $"Request '{request.Method}' towards '{request.RequestUri?.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped)}{request.RequestUri!.PathAndQuery}' failed after {timespan.TotalMilliseconds:F1}ms");
    }

    private static HttpRequestMessage PrepareRequestMessage()
    {
        var request = new HttpRequestMessage();
        request.Method = HttpMethod.Post;
        request.RequestUri = new Uri("http://127.0.0.1/elo?q=10");
        return request;
    }

    private static HttpResponseMessage PrepareResponseMessage()
    {
        var response = new HttpResponseMessage();
        response.StatusCode = HttpStatusCode.Created;

        return response;
    }
}