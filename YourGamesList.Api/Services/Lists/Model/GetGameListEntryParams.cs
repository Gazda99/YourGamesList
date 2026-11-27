using System;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Lists;

namespace YourGamesList.Api.Services.Lists.Model;

public class GetGameListEntryParams
{
    public required UserInformationToken UserInformation { get; set; }
    public required Guid GamesListId { get; set; }
    public required Guid EntryId { get; set; }
}

public static class GetGameListEntryParamsMappingExtensions
{
    public static GetGameListEntryParams ToParams(this GetGameListEntryRequest request)
    {
        return new GetGameListEntryParams
        {
            UserInformation = request.UserInformation,
            GamesListId = request.ListId,
            EntryId = request.EntryId
        };
    }
}
