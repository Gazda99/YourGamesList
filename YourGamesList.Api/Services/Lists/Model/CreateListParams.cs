using YourGamesList.Api.Model.Requests.Lists;

namespace YourGamesList.Api.Services.Lists.Model;

public class CreateListParams
{
    public required string Name { get; set; }
}

public static class CreateListParamsMappingExtensions
{
    public static CreateListParams ToParams(this CreateListRequest request)
    {
        return new CreateListParams
        {
            Name = request.Body.Name,
        };
    }
}