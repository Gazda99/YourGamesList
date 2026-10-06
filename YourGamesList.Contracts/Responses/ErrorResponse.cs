using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace YourGamesList.Contracts.Responses;

public class ErrorResponse
{
    [JsonPropertyName("errors")] public IEnumerable<Error> Errors { get; init; } = [];
}

public class Error
{
    [JsonPropertyName("code")] public required string Code { get; init; }
    [JsonPropertyName("description")] public string? Description { get; init; }
}