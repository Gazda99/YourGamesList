using System;
using YourGamesList.Api.Model.Requests.Lists;

namespace YourGamesList.Api.Services.Lists.Model;

public class DeleteListParams
{
    public required Guid Id { get; set; }
}

public static class DeleteListParamsMappingExtensions
{
    public static DeleteListParams ToParams(this DeleteListRequest request)
    {
        return new DeleteListParams
        {
            Id = request.Body.Id,
        };
    }
}