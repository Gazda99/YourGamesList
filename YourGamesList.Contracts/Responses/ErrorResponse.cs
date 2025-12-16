using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace YourGamesList.Contracts.Responses;

public class ErrorResponse
{
    [JsonPropertyName("errors")] public IEnumerable<string> Errors { get; init; } = [];
}