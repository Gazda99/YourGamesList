using System;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Lists;
using YourGamesList.Contracts;
using YourGamesList.Contracts.Dto;

namespace YourGamesList.Api.Services.Lists.Model;

public class UpdateGameListEntryParams
{
    public required UserInformationToken UserInformation { get; set; }
    public required Guid GamesListId { get; set; }
    public required Guid Id { get; set; }
    public Optional<CompletionStatusDto?> CompletionStatus { get; set; }
    public Optional<string?> Description { get; set; }
    public Optional<byte?> Rating { get; set; }
}

public static class UpdateGameListEntryParamsMappingExtensions
{
    public static UpdateGameListEntryParams ToParams(this UpdateGameListEntryRequest request)
    {
        return new UpdateGameListEntryParams
        {
            UserInformation = request.UserInformation,
            GamesListId = request.ListId,
            Id = request.EntryId,
            CompletionStatus = request.Body.CompletionStatus,
            Description = request.Body.Description,
            Rating = request.Body.Rating
        };
    }
}
