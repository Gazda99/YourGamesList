using System.Text.Json.Serialization;

namespace YourGamesList.Contracts.Dto;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TypeOfDateDto
{
    None,
    Exact,
    Before,
    After
}