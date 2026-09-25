namespace YourGamesList.Contracts.Requests.Users;

public sealed record AuthUserLoginRequestBody(
    string Username,
    string Password
);