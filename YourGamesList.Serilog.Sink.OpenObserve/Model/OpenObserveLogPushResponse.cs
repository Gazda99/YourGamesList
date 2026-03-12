using System.Text.Json.Serialization;

namespace YourGamesList.Serilog.Sink.OpenObserve.Model;

public class OpenObserveLogPushResponse
{
    [JsonPropertyName("code")] public int Code { get; set; }
    [JsonPropertyName("error")] public int Error { get; set; }
    [JsonPropertyName("status")] public OpenObserveLogPushResponseStatus[] Status { get; set; } = [];
}

public class OpenObserveLogPushResponseStatus
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("successful")] public int Successful { get; set; }
    [JsonPropertyName("failed")] public int Failed { get; set; }
    [JsonPropertyName("error")] public int Error { get; set; }
}