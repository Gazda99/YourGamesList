using System;
using YourGamesList.Api.Model;
using YourGamesList.Contracts.Dto;


namespace YourGamesList.Api.Services.Ygl.Lists.Model;

public class AddEntriesToListParameter
{
    public required UserInformationToken UserInformation { get; init; }
    public Guid ListId { get; init; }
    public EntryToAddParameter[] EntriesToAdd { get; init; } = [];
}

public class EntryToAddParameter
{
    public required long GameId { get; init; }

    public string? Description { get; set; }
    public bool? IsStarred { get; set; }
    public byte? Rating { get; set; }
    public CompletionStatusDto? CompletionStatus { get; set; }
}