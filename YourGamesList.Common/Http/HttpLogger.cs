using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
                $"Sending '{request.Method}' to '{request.RequestUri?.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped)}{request.RequestUri!.PathAndQuery}'");
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

        _logger.LogInformation($"Received '{(int) response.StatusCode} {response.StatusCode}' after {elapsed.TotalMilliseconds:F1}ms");
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
            $"Request '{request.Method}' towards '{request.RequestUri?.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped)}{request.RequestUri!.PathAndQuery}' failed after {elapsed.TotalMilliseconds:F1}ms");
    }

    private static bool TryGetLogScopesFromHttpHeaders(System.Net.Http.Headers.HttpHeaders? headers, Dictionary<string, string> headersToLog,
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
            ["Response.ElapsedMilliseconds"] = elapsed.TotalMilliseconds.ToString("F1")
        };
    }

    private static Dictionary<string, object> RequestFailedScopes(HttpRequestMessage request, TimeSpan elapsed)
    {
        return new Dictionary<string, object>
        {
            ["Request.Method"] = request.Method,
            ["Request.Host"] = request.RequestUri?.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped) ?? "N/A",
            ["Request.Path"] = request.RequestUri!.PathAndQuery,
            ["Response.ElapsedMilliseconds"] = elapsed.TotalMilliseconds.ToString("F1")
        };
    }
}