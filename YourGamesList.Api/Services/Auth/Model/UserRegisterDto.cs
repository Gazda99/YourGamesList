using YourGamesList.Api.Model.Requests.Users;

namespace YourGamesList.Api.Services.Auth.Model;

public class UserRegisterDto
{
    public required string Email { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
}

public static class UserRegisterDtoMappingExtensions
{
    public static UserRegisterDto ToDto(this UserRegisterRequest request)
    {
        return new UserRegisterDto
        {
            Email = request.Body.Email,
            Username = request.Body.Username,
            Password = request.Body.Password
        };
    }
}