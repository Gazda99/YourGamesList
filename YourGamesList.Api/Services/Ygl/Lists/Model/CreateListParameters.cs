using YourGamesList.Api.Model;

namespace YourGamesList.Api.Services.Ygl.Lists.Model;

public class CreateListParameters
{
    public required JwtUserInformation UserInformation { get; init; }
    public required string ListName { get; init; }
    public string? Description { get; init; } 
    public bool? IsPublic { get; init; } = false;
}