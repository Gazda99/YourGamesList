using YourGamesList.Api.Model.Requests.Users;

namespace YourGamesList.Api.Services.Auth.Model;

public class UserLoginDto
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}

public static class UserLoginDtoMappingExtensions
{
    public static UserLoginDto ToDto(this UserLoginRequest request)
    {
        return new UserLoginDto
        {
            Username = request.Body.Username,
            Password = request.Body.Password
        };
    }
}