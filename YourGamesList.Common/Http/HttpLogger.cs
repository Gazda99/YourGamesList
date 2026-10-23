using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Http.Logging;
using Microsoft.Extensions.Logging;

namespace YourGamesList.Common.Http;

public class HttpLogger : IHttpClientLogger
{
    private readonly ILogger<HttpLogger> _logger;

    public HttpLoggerConfiguration Options { get; set; } = new HttpLoggerConfiguration();

    public HttpLogger(ILogger<HttpLogger> logger)
    {
        _logger = logger;
    }

    public object? LogRequestStart(HttpRequestMessage request)
    {
        using var requestHeaderScope = TryGetLogScopesFromHttpHeaders(request.Headers, Options.CustomRequestHeadersToLog, out var headerData)
            ? _logger.BeginScope(headerData)
            : null;
        using (_logger.BeginScope(RequestStartScopes(request)))
        {
            _logger.LogInformation(
                "Sending {RequestMethod} to {RequestHost}{RequestPath}",
                request.Method,
                request.RequestUri?.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped),
                request.RequestUri!.PathAndQuery);
        }

        return null;
    }

    public void LogRequestStop(object? context, HttpRequestMessage request, HttpResponseMessage response, TimeSpan elapsed)
    {
        using var requestHeaderScope = TryGetLogScopesFromHttpHeaders(request.Headers, Options.CustomRequestHeadersToLog, out var requestHeaderData)
            ? _logger.BeginScope(requestHeaderData)
            : null;
        using var responseHeaderScope = TryGetLogScopesFromHttpHeaders(response.Headers, Options.CustomResponseHeadersToLog, out var responseHeaderData)
            ? _logger.BeginScope(responseHeaderData)
            : null;
        using var requestStopScopes = _logger.BeginScope(RequestStopScopes(response, elapsed));

        _logger.LogInformation(
            "Received {ResponseStatusCodeInt} {ResponseStatusCode} after {ElapsedMilliseconds}ms",
            (int) response.StatusCode,
            response.StatusCode,
            FormatElapsedMilliseconds(elapsed));
    }

    public void LogRequestFailed(object? context, HttpRequestMessage request, HttpResponseMessage? response, Exception exception, TimeSpan elapsed)
    {
        using var requestHeaderScope = TryGetLogScopesFromHttpHeaders(request.Headers, Options.CustomRequestHeadersToLog, out var requestHeaderData)
            ? _logger.BeginScope(requestHeaderData)
            : null;
        using var responseHeaderScope = TryGetLogScopesFromHttpHeaders(response?.Headers, Options.CustomResponseHeadersToLog, out var responseHeaderData)
            ? _logger.BeginScope(responseHeaderData)
            : null;
        using var requestFailedScopes = _logger.BeginScope(RequestFailedScopes(request, elapsed));

        _logger.LogWarning(exception,
            "Request {RequestMethod} towards {RequestHost}{RequestPath} failed after {ElapsedMilliseconds}ms",
            request.Method,
            request.RequestUri?.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped),
            request.RequestUri!.PathAndQuery,
            FormatElapsedMilliseconds(elapsed));
    }

    private static bool TryGetLogScopesFromHttpHeaders(HttpHeaders? headers, Dictionary<string, string> headersToLog,
        out Dictionary<string, object> scopes)
    {
        scopes = [];

        if (headers == null || !headers.Any())
        {
            return false;
        }

        foreach (var (headerValue, logPropName) in headersToLog)
        {
            var found = headers.TryGetValues(headerValue, out var headerValues);
            if (!found || headerValues == null || !headerValues.Any())
            {
                continue;
            }

            var scopeValue = string.Join(", ", headerValues);
            scopes[logPropName] = scopeValue;
        }

        return scopes.Count != 0;
    }


    private static Dictionary<string, object> RequestStartScopes(HttpRequestMessage request)
    {
        return new Dictionary<string, object>
        {
            ["Request.Method"] = request.Method,
            ["Request.Host"] = request.RequestUri?.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped) ?? "N/A",
            ["Request.Path"] = request.RequestUri!.PathAndQuery
        };
    }

    private static Dictionary<string, object> RequestStopScopes(HttpResponseMessage response, TimeSpan elapsed)
    {
        return new Dictionary<string, object>
        {
            ["Response.StatusCodeInt"] = (int) response.StatusCode,
            ["Response.StatusCodeString"] = response.StatusCode,
            ["Response.ElapsedMilliseconds"] = FormatElapsedMilliseconds(elapsed)
        };
    }

    private static Dictionary<string, object> RequestFailedScopes(HttpRequestMessage request, TimeSpan elapsed)
    {
        return new Dictionary<string, object>
        {
            ["Request.Method"] = request.Method,
            ["Request.Host"] = request.RequestUri?.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped) ?? "N/A",
            ["Request.Path"] = request.RequestUri!.PathAndQuery,
            ["Response.ElapsedMilliseconds"] = FormatElapsedMilliseconds(elapsed)
        };
    }

    private static string FormatElapsedMilliseconds(TimeSpan elapsed) =>
        elapsed.TotalMilliseconds.ToString("F1", CultureInfo.InvariantCulture);
}