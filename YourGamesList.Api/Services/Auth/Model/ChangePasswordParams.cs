using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Users;

namespace YourGamesList.Api.Services.Auth.Model;

public class ChangePasswordParams
{
    public required UserInformationToken UserInformation { get; set; }
    public required string CurrentPassword { get; set; }
    public required string NewPassword { get; set; }
}

public static class ChangePasswordParamsMappingExtensions
{
    public static ChangePasswordParams ToParams(this UserChangePasswordRequest request)
    {
        return new ChangePasswordParams
        {
            UserInformation = request.UserInformation,
            CurrentPassword = request.Body.CurrentPassword,
            NewPassword = request.Body.NewPassword
        };
    }
}
