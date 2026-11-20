namespace YourGamesList.Contracts.Requests.Users;

public sealed record AuthConfirmEmailRequestBody(
    string Token
);