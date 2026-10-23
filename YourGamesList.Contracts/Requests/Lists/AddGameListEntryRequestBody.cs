using YourGamesList.Contracts.Dto;

namespace YourGamesList.Contracts.Requests.Lists;

public record AddGameListEntryRequestBody
{
    public required CompletionStatusDto CompletionStatus { get; init; }
    public required long IgdbId { get; init; }
    public string? Description { get; init; }
    public byte? Rating { get; init; }
}
