using System;
using YourGamesList.Api.Model;

namespace YourGamesList.Api.Services.Ygl.Lists.Model;

public class DeleteListEntriesParameter
{
    public required JwtUserInformation UserInformation { get; init; }
    public Guid ListId { get; init; }
    public Guid[] EntriesToRemove { get; init; } = [];
}