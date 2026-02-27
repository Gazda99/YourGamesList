using System;
using System.Collections.Generic;

namespace YourGamesList.Contracts.Dto;

public class GameListEntryDto
{
    public required Guid Id { get; init; }
    public required DateTimeOffset CreatedDate { get; init; }
    public required DateTimeOffset? LastModifiedDate { get; init; }

    public required GameDto? Game { get; init; }
    public required string Description { get; init; }
    public required List<OwnershipInfoDto> OwnershipInfo { get; init; }
    public required bool IsStarred { get; init; }
    public required byte? Rating { get; init; }
    public required CompletionStatusDto CompletionStatus { get; init; }
}