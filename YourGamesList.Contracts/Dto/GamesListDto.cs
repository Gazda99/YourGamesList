using System;
using System.Collections.Generic;

namespace YourGamesList.Contracts.Dto;

public class GamesListDto
{
    public required DateTimeOffset CreatedAt { get; set; }
    public string? Description { get; set; }
    public required bool IsPrivate { get; set; }
    public required string Name { get; set; }
    public IEnumerable<GamesListEntryDto> GameListEntries { get; } = [];
}