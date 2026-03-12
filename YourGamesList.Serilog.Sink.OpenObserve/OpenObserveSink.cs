using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;
using YourGamesList.Serilog.Sink.OpenObserve.Model;

namespace YourGamesList.Serilog.Sink.OpenObserve;

public class OpenObserveSink : IBatchedLogEventSink
{
    private readonly HttpClient _httpClient;
    private readonly string _auth;
    private readonly string _endpoint;

    public OpenObserveSink(
        string url,
        string organization,
        string login,
        string secret,
        string streamName = "default"
    )
    {
        _httpClient = new HttpClient()
        {
            BaseAddress = new Uri(url)
        };

        var tmpAuth = $"{login}:{secret}";
        _auth = Convert.ToBase64String(Encoding.ASCII.GetBytes(tmpAuth));

        _endpoint = Endpoint(organization, streamName);
    }

    public async Task EmitBatchAsync(IEnumerable<LogEvent> batch)
    {
        var sw = new StringWriter();
        var batchList = batch.ToList();
        var formatter = new LogFormatter();

        foreach (var logEvent in batchList)
        {
            formatter.Format(logEvent, sw);
        }

        var payload = sw.ToString();

        var logPushResultRaw = await Send(payload);
        var logPushResult = JsonSerializer.Deserialize<OpenObserveLogPushResponse>(logPushResultRaw);
        if (logPushResult == null)
        {
            throw new Exception("Failed to deserialize OpenObserve response.");
        }

        if (logPushResult.Code != 200)
        {
            var resPrint = PrintResponse(logPushResult);
            Console.WriteLine(resPrint);
            throw new Exception($"Failed to send log event to OpenObserve. Response: {resPrint}");
        }
    }

    public Task OnEmptyBatchAsync()
    {
        return Task.CompletedTask;
    }

    private static string PrintResponse(OpenObserveLogPushResponse logPushResult)
    {
        var sb = new StringBuilder();
        sb.Append($"Code: {logPushResult.Code}, ");
        sb.Append($"Error: {logPushResult.Error}");
        if (logPushResult.Status.Length == 0)
        {
            return sb.ToString();
        }

        sb.Append("Status: [");
        foreach (var status in logPushResult.Status)
        {
            sb.Append($"{{Name: {status.Name}, ");
            sb.Append($"Successful: {status.Successful}, ");
            sb.Append($"Failed: {status.Failed}, ");
            sb.Append($"Error: {status.Error}}}");
        }

        sb.Append(']');

        return sb.ToString();
    }

    private async Task<string> Send(string data)
    {
        const string mediaType = "application/json";

        var requestBody = new StringContent(data, Encoding.UTF8, mediaType);
        var req = new HttpRequestMessage(HttpMethod.Post, _endpoint);
        req.Content = requestBody;
        req.Headers.Authorization = new AuthenticationHeaderValue("Basic", _auth);

        var res = await _httpClient.SendAsync(req);

        var responseContent = await res.Content.ReadAsStringAsync();
        return responseContent;
    }

    private static string Endpoint(string organization, string streamName)
    {
        return $"/api/{organization}/{streamName}/_multi";
    }
}