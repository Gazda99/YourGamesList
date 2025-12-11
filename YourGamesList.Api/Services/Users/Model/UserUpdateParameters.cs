using System;
using YourGamesList.Api.Model;

namespace YourGamesList.Api.Services.Users.Model;

public class UserUpdateParameters
{
    public required JwtUserInformation UserInformation { get; init; }
    public string? Description { get; set; }
    public DateTimeOffset? DateOfBirth { get; set; }
    public string? Country { get; set; }
}