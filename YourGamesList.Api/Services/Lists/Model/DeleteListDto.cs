using System;
using YourGamesList.Api.Model.Requests.Lists;

namespace YourGamesList.Api.Services.Lists.Model;

public class DeleteListDto
{
    public required Guid Id { get; set; }
}

public static class DeleteListDtoMappingExtensions
{
    public static DeleteListDto ToDto(this DeleteListRequest request)
    {
        return new DeleteListDto
        {
            Id = request.Body.Id,
        };
    }
}