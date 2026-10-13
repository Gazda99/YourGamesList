using YourGamesList.Api.Model.Requests.Users;

namespace YourGamesList.Api.Services.Auth.Model;

public class UserLoginParams
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}

public static class UserLoginParamsMappingExtensions
{
    public static UserLoginParams ToParams(this UserLoginRequest request)
    {
        return new UserLoginParams
        {
            Username = request.Body.Username.Trim(),
            Password = request.Body.Password
        };
    }
}