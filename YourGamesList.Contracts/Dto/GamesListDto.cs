using System;
using System.Collections.Generic;

namespace YourGamesList.Contracts.Dto;

public class GamesListDto
{
    public required Guid Id { get; init; }
    public required string Desc { get; init; }
    public required string Name { get; init; }
    public required bool IsPublic { get; init; }
    public required bool CanBeDeleted { get; init; }
    public required List<GameListEntryDto> Entries { get; init; }
    public required DateTimeOffset CreatedDate { get; init; }
    public required DateTimeOffset? LastModifiedDate { get; init; }
}