using System;

namespace YourGamesList.Contracts.Requests.Users;

public class UserUpdateRequestBody
{
    public string? Description { get; set; } = null;
    public DateTimeOffset? DateOfBirth { get; set; } = null;
    public string? Country { get; set; } = null;
}