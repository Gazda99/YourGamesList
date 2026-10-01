using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Users;

namespace YourGamesList.Api.Services.YourGamesList.Model;

public class UserGetSelfParams
{
    public required UserInformationToken UserInformation { get; set; }
}

public static class UserGetSelfParamsMappingExtensions
{
    public static UserGetSelfParams ToParams(this UserGetSelfRequest request)
    {
        return new UserGetSelfParams
        {
            UserInformation = request.UserInformation
        };
    }
}