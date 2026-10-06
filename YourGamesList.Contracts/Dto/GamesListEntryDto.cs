using System.Collections.Generic;

namespace YourGamesList.Contracts.Dto;

public class GamesListEntryDto
{
    public required CompletionStatusDto CompletionStatus { get; set; }
    public string? Description { get; set; }
    public required long IgdbId { get; set; }
    public byte? Rating { get; set; }
    public List<OwnershipInfoDto> OwnershipInfos { get; set; }= [];
    public GameDto? Game { get; set; }
}