using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Lists;

namespace YourGamesList.Api.Services.Lists.Model;

public class CreateListParams
{
    public required UserInformationToken UserInformation { get; set; }
    public required string Name { get; set; }
}

public static class CreateListParamsMappingExtensions
{
    public static CreateListParams ToParams(this CreateListRequest request)
    {
        return new CreateListParams
        {
            UserInformation = request.UserInformation,
            Name = request.Body.Name.Trim()
        };
    }
}