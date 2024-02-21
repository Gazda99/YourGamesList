using System.Net.Http.Headers;
using System.Text;

namespace YourGamesList.Common.Http;

public class HttpRequestMessageBuilder
{
    private readonly HttpRequestMessage _httpRequestMessage = new();

    public static HttpRequestMessageBuilder Create => new();

    public HttpRequestMessageBuilder WithMethod(HttpMethod method)
    {
        _httpRequestMessage.Method = method;
        return this;
    }

    public HttpRequestMessageBuilder WithUri(string uri, IEnumerable<KeyValuePair<string, string>>? parameters = null,
        UriKind uriKind = UriKind.Absolute)
    {
        if (parameters != null)
        {
            var constructedParameters = ConstructParameters(parameters);
            if (!string.IsNullOrEmpty(constructedParameters)) uri = $"{uri}{constructedParameters}";
        }

        _httpRequestMessage.RequestUri = new Uri(uri, uriKind);
        return this;
    }

    public HttpRequestMessageBuilder WithHeader(string key, string val)
    {
        _httpRequestMessage.Headers.Add(key, val);
        return this;
    }

    public HttpRequestMessageBuilder WithHeader((string key, string val) headerTuple)
    {
        _httpRequestMessage.Headers.Add(headerTuple.key, headerTuple.val);
        return this;
    }

    public HttpRequestMessageBuilder WithHeaders(IEnumerable<KeyValuePair<string, string>> headers)
    {
        foreach (var header in headers) _httpRequestMessage.Headers.Add(header.Key, header.Value);

        return this;
    }

    public HttpRequestMessageBuilder WithBearerToken(string bearerToken)
    {
        _httpRequestMessage.Headers.Add("Authorization", $"Bearer {bearerToken}");
        return this;
    }


    public HttpRequestMessageBuilder WithStringContent(string content, string? mediaType = null)
    {
        _httpRequestMessage.Content = string.IsNullOrEmpty(mediaType)
            ? new StringContent(content)
            : new StringContent(content, new MediaTypeHeaderValue(mediaType));

        return this;
    }

    public HttpRequestMessageBuilder WithFormUrlEncodedContent(IEnumerable<KeyValuePair<string, string>> content)
    {
        _httpRequestMessage.Content = new FormUrlEncodedContent(content);
        return this;
    }

    public HttpRequestMessageBuilder WithContent(HttpContent content)
    {
        _httpRequestMessage.Content = content;
        return this;
    }


    private static string ConstructParameters(IEnumerable<KeyValuePair<string, string>> uriParameters)
    {
        if (!uriParameters.Any())
            return string.Empty;

        var sb = new StringBuilder("?");
        foreach (var p in uriParameters)
        {
            sb.Append(p.Key);
            sb.Append('=');
            sb.Append(p.Value);
            sb.Append('&');
        }

        sb.Length--;
        var parameters = sb.ToString();

        return parameters;
    }

    public HttpRequestMessage Build()
    {
        return _httpRequestMessage;
    }
}