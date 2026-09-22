namespace YourGamesList.Contracts.Requests.Users;

public class AuthUserRegisterRequestBody
{
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}