using System;
using System.Collections.Generic;

namespace YourGamesList.Contracts.Dto;

public class GamesListDto
{
    public required DateTimeOffset CreatedAt { get; set; }
    public string? Description { get; set; }
    public required Guid Id { get; set; }
    public DateTimeOffset? ModifiedAt { get; set; }
    public required bool Editable { get; set; }
    public required bool IsPrivate { get; set; }
    public required string Name { get; set; }
    public List<GamesListEntryDto> GameListEntries { get; set; } = [];
}