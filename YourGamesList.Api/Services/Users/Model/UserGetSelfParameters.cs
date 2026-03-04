using YourGamesList.Api.Model;

namespace YourGamesList.Api.Services.Users.Model;

public class UserGetSelfParameters
{
    public required UserInformationToken UserInformation { get; init; }
}