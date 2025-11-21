namespace YourGamesList.Contracts.Requests.Users;

public class AuthUserLoginRequestBody
{
    public required string Username { get; init; }
    public required string Password { get; init; }
}