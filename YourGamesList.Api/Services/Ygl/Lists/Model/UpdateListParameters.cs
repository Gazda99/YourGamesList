using System;
using YourGamesList.Api.Model;

namespace YourGamesList.Api.Services.Ygl.Lists.Model;

public class UpdateListParameters
{
    public required JwtUserInformation UserInformation { get; init; }
    public Guid ListId { get; init; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsPublic { get; set; }
}