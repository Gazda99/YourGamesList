using System;

namespace YourGamesList.Contracts.Requests.Users;

public class UserUpdateRequestBody
{
    public string? Description { get; set; }
    public DateTimeOffset? DateOfBirth { get; set; }
    public string? Country { get; set; }
}