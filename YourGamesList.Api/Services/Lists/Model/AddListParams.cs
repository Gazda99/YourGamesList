using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Lists;

namespace YourGamesList.Api.Services.Lists.Model;

public class AddListParams
{
    public required UserInformationToken UserInformation { get; set; }
    public required string Name { get; set; }
}

public static class AddListParamsMappingExtensions
{
    public static AddListParams ToParams(this AddListRequest request)
    {
        return new AddListParams
        {
            UserInformation = request.UserInformation,
            Name = request.Body.Name.Trim()
        };
    }
}