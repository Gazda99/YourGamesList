using System;

namespace YourGamesList.Contracts.Dto;

public class UserDto
{
    public required Guid Id { get; init; }
    public required string Username { get; init; }
    public required string Description { get; init; } = string.Empty;
    public required DateTimeOffset? DateOfBirth { get; init; }
    public required string Country { get; init; } = string.Empty;
}