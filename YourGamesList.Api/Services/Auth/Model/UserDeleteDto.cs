using YourGamesList.Api.Model.Requests.Users;

namespace YourGamesList.Api.Services.Auth.Model;

public class UserDeleteDto
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}

public static class UserDeleteDtoMappingExtensions
{
    public static UserDeleteDto ToDto(this UserDeleteRequest request)
    {
        return new UserDeleteDto
        {
            Username = request.Body.Username,
            Password = request.Body.Password
        };
    }
}