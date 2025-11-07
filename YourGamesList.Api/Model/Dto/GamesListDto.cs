using System;
using System.Collections.Generic;

namespace YourGamesList.Api.Model.Dto;

public class GamesListDto
{
    public required Guid Id { get; init; }
    public required string Desc { get; init; }
    public required string Name { get; init; }
    public required bool IsPublic { get; init; }
    public required bool CanBeDeleted { get; init; }
    public required List<GameListEntryDto> Entries { get; init; }
}