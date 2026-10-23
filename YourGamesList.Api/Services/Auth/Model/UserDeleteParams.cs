using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Users;

namespace YourGamesList.Api.Services.Auth.Model;

public class UserDeleteParams
{
    public required UserInformationToken UserInformation { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
}

public static class UserDeleteParamsMappingExtensions
{
    public static UserDeleteParams ToParams(this UserDeleteRequest request)
    {
        return new UserDeleteParams
        {
            UserInformation = request.UserInformation,
            Username = request.Body.Username.Trim(),
            Password = request.Body.Password
        };
    }
}