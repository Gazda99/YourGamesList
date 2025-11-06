using System;

namespace YourGamesList.Api.Services.Ygl.Lists.Model;

public class UpdateListsParameters
{
    public Guid Id { get; init; }
    public string? Name { get; set; }
    public string? Desc { get; set; }
    public bool? IsPublic { get; set; }
}