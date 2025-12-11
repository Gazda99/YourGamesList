using System.Text.Json.Serialization;

namespace YourGamesList.Contracts.Dto;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CompletionStatusDto
{
    Unspecified = 0,
    JustTried = 10,
    Played = 20,
    FullyCompleted = 30,
}