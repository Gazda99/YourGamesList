using System;
using YourGamesList.Contracts.Dto;

namespace YourGamesList.Contracts.Requests.Lists;

public class AddEntriesToListRequestBody
{
    public required Guid ListId { get; init; }
    public EntryToAddRequestPart[] EntriesToAdd { get; init; } = [];
}

public class EntryToAddRequestPart
{
    public required Guid GameId { get; init; }

    public string? Desc { get; set; }
    public PlatformDto[]? Platforms { get; set; }
    public GameDistributionDto[]? GameDistributions { get; set; }
    public bool? IsStarred { get; set; }
    public byte? Rating { get; set; }
    public CompletionStatusDto? CompletionStatus { get; set; }
}