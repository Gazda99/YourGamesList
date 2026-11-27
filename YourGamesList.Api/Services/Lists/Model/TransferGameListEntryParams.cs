using System;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Lists;

namespace YourGamesList.Api.Services.Lists.Model;

public class TransferGameListEntryParams
{
    public required UserInformationToken UserInformation { get; set; }
    public required Guid GamesListId { get; set; }
    public required Guid Id { get; set; }
    public required Guid TargetGamesListId { get; set; }
    public required bool Move { get; set; }
}

public static class TransferGameListEntryParamsMappingExtensions
{
    public static TransferGameListEntryParams ToParams(this TransferGameListEntryRequest request)
    {
        return new TransferGameListEntryParams
        {
            UserInformation = request.UserInformation,
            GamesListId = request.ListId,
            Id = request.EntryId,
            TargetGamesListId = request.Body.TargetListId,
            Move = request.Body.Move
        };
    }
}
