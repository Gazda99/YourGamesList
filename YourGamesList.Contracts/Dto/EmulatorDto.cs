using System.Text.Json.Serialization;

namespace YourGamesList.Contracts.Dto;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EmulatorDto
{
    Unspecified = 0,
    Xenia = 100,
    PPSSPP = 200,
    RPCS3 = 201,
    CEMU = 300
}