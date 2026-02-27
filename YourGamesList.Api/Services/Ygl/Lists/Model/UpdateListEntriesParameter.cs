using System;
using YourGamesList.Api.Model;
using YourGamesList.Contracts.Dto;


namespace YourGamesList.Api.Services.Ygl.Lists.Model;

public class UpdateListEntriesParameter
{
    public required JwtUserInformation UserInformation { get; init; }
    public Guid ListId { get; init; }
    public EntryToUpdateParameter[] EntriesToUpdate { get; init; } = [];
}

public class EntryToUpdateParameter
{
    public required Guid EntryId { get; init; }
    public string? Description { get; set; }
    public bool? IsStarred { get; set; }
    public byte? Rating { get; set; }
    public CompletionStatusDto? CompletionStatus { get; set; }
}