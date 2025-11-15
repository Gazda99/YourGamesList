using System;

namespace YourGamesList.Api.Model.Dto;

public class UserDto
{
    public required Guid Id { get; init; }
    public required string Username { get; set; }
    public required string Description { get; set; } = string.Empty;
    public required DateTimeOffset? DateOfBirth { get; set; }
    public required string Country { get; set; } = string.Empty;
}