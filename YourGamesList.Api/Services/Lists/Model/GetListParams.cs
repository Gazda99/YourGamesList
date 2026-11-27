using System;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Lists;

namespace YourGamesList.Api.Services.Lists.Model;

public class GetListParams
{
    public required UserInformationToken UserInformation { get; set; }
    public required Guid GamesListId { get; set; }
}

public static class GetListParamsMappingExtensions
{
    public static GetListParams ToParams(this GetListRequest request)
    {
        return new GetListParams
        {
            UserInformation = request.UserInformation,
            GamesListId = request.ListId
        };
    }
}
