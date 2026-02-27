using System;

namespace YourGamesList.Contracts.Requests.Lists;

public class UpdateListRequestBody
{
    public required Guid ListId { get; init; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsPublic { get; set; }
}