using System;

namespace YourGamesList.Contracts.Requests.Users;

public sealed record AuthUserRegisterRequestBody(
    string Username,
    string Email,
    string Password,
    DateOnly DateOfBirth
);