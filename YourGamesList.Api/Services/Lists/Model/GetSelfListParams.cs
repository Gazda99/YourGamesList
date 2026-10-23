using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Lists;

namespace YourGamesList.Api.Services.Lists.Model;

public class GetSelfListParams
{
    public required UserInformationToken UserInformation { get; set; }
}

public static class GetSelfListParamsMappingExtensions
{
    public static GetSelfListParams ToParams(this GetSelfListRequest request)
    {
        return new GetSelfListParams
        {
            UserInformation = request.UserInformation
        };
    }
}