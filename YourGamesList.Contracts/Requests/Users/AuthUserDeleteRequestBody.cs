namespace YourGamesList.Contracts.Requests.Users;

public sealed record AuthUserDeleteRequestBody(
    string Username,
    string Password
);