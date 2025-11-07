using System;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Dto;

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
    public string? Desc { get; set; }
    public PlatformDto[]? Platforms { get; set; }
    public GameDistributionDto[]? GameDistributions { get; set; }
    public bool? IsStarred { get; set; }
    public byte? Rating { get; set; }
    public CompletionStatusDto? CompletionStatus { get; set; }
}