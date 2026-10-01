using System;

namespace YourGamesList.Contracts.Dto;

public class UserDto
{
    public required Guid Id { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
    public required string Username { get; set; }
    public required string? Description { get; set; }
    public required DateOnly DateOfBirth { get; set; }
}