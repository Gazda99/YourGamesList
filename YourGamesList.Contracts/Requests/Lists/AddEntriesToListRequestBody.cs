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
    public required long GameId { get; init; }

    public string? Description { get; set; }
    public bool? IsStarred { get; set; }
    public byte? Rating { get; set; }
    public CompletionStatusDto? CompletionStatus { get; set; }
}