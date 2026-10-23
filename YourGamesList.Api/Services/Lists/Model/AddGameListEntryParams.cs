using System;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Lists;
using YourGamesList.Contracts.Dto;

namespace YourGamesList.Api.Services.Lists.Model;

public class AddGameListEntryParams
{
    public required UserInformationToken UserInformation { get; set; }
    public required Guid GamesListId { get; set; }
    public required CompletionStatusDto CompletionStatus { get; set; }
    public required long IgdbId { get; set; }
    public string? Description { get; set; }
    public byte? Rating { get; set; }
}

public static class AddGameListEntryParamsMappingExtensions
{
    public static AddGameListEntryParams ToParams(this AddGameListEntryRequest request)
    {
        return new AddGameListEntryParams
        {
            UserInformation = request.UserInformation,
            GamesListId = request.ListId,
            CompletionStatus = request.Body.CompletionStatus,
            IgdbId = request.Body.IgdbId,
            Description = request.Body.Description,
            Rating = request.Body.Rating
        };
    }
}