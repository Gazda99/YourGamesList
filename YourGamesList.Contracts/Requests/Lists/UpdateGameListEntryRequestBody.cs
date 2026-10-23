using YourGamesList.Contracts.Dto;

namespace YourGamesList.Contracts.Requests.Lists;

public record UpdateGameListEntryRequestBody
{
    public Optional<CompletionStatusDto?> CompletionStatus { get; init; }
    public Optional<string?> Description { get; init; }
    public Optional<byte?> Rating { get; init; }
}