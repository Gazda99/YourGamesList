using System;
using YourGamesList.Contracts.Dto;

namespace YourGamesList.Contracts.Requests.Lists;

public class UpdateListEntriesRequestBody
{
    public required Guid ListId { get; init; }
    public EntryToUpdateRequestPart[] EntriesToUpdate { get; init; } = [];
}

public class EntryToUpdateRequestPart
{
    public required Guid EntryId { get; init; }

    public string? Desc { get; set; }
    public PlatformDto[]? Platforms { get; set; }
    public GameDistributionDto[]? GameDistributions { get; set; }
    public bool? IsStarred { get; set; }
    public byte? Rating { get; set; }
    public CompletionStatusDto? CompletionStatus { get; set; }
}