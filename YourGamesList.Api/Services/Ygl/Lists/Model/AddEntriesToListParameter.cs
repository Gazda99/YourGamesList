using System;
using YourGamesList.Api.Model;
using YourGamesList.Contracts.Dto;


namespace YourGamesList.Api.Services.Ygl.Lists.Model;

public class AddEntriesToListParameter
{
    public required JwtUserInformation UserInformation { get; init; }
    public Guid ListId { get; init; }
    public EntryToAddParameter[] EntriesToAdd { get; init; } = [];
}

public class EntryToAddParameter
{
    public required Guid GameId { get; init; }

    public string? Desc { get; set; }
    public PlatformDto[]? Platforms { get; set; }
    public GameDistributionDto[]? GameDistributions { get; set; }
    public bool? IsStarred { get; set; }
    public byte? Rating { get; set; }
    public CompletionStatusDto? CompletionStatus { get; set; }
}