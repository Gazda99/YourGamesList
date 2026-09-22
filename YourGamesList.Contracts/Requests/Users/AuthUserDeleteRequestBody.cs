namespace YourGamesList.Contracts.Requests.Users;

public class AuthUserDeleteRequestBody
{
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}