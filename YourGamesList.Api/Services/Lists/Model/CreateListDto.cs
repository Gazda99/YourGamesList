using YourGamesList.Api.Model.Requests.Lists;

namespace YourGamesList.Api.Services.Lists.Model;

public class CreateListDto
{
    public required string Name { get; set; }
}

public static class CreateListDtoMappingExtensions
{
    public static CreateListDto ToDto(this CreateListRequest request)
    {
        return new CreateListDto
        {
            Name = request.Body.Name,
        };
    }
}