using System;

namespace YourGamesList.Api.Model.Dto;

public class GameListEntryDto
{
    public required Guid Id { get; init; }
    public required GameDto Game { get; init; }
    public required string Desc { get; init; }
    public required PlatformDto[] Platforms { get; init; }
    public required GameDistributionDto[] GameDistributions { get; init; }
    public required bool IsStarred { get; init; }
    public required byte Rating { get; init; }
    public required CompletionStatusDto CompletionStatus { get; init; }
}