using System.Text.Json.Serialization;

namespace YourGamesList.Contracts.Dto;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PlatformDto
{
    Unspecified = 0,
    PC = 100,
    PlayStation1 = 200,
    PlayStation2 = 201,
    PlayStation3 = 202,
    PlayStation4 = 203,
    PlayStation5 = 204,
    PSP = 220,
    PSVita = 221,
    Xbox = 300,
    Xbox360 = 301,
    XboxOne = 302,
    XboxSeriesX = 303,
    XboxSeriesS = 304,
    NintendoSwitch = 402,
    AndroidMobileDevice = 500,
    IOSMobileDevice = 501
}