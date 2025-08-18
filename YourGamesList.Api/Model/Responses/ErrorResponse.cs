using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace YourGamesList.Api.Model.Responses;

public class ErrorResponse
{
    [JsonPropertyName("errors")] public IEnumerable<string> Errors { get; init; } = [];
}