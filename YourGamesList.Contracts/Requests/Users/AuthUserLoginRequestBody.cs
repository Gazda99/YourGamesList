namespace YourGamesList.Contracts.Requests.Users;

public class AuthUserLoginRequestBody
{
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}