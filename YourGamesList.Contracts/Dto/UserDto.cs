using System;

namespace YourGamesList.Contracts.Dto;

public class UserDto
{
    public required Guid Id { get; set; }
    public required string Username { get; set; }
}