using System;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Lists;

namespace YourGamesList.Api.Services.Lists.Model;

public class DeleteListParams
{
    public required UserInformationToken UserInformation { get; set; }
    public required Guid Id { get; set; }
}

public static class DeleteListParamsMappingExtensions
{
    public static DeleteListParams ToParams(this DeleteListRequest request)
    {
        return new DeleteListParams
        {
            UserInformation = request.UserInformation,
            Id = request.ListId,
        };
    }
}