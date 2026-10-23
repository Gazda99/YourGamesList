using System;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Lists;

namespace YourGamesList.Api.Services.Lists.Model;

public class RemoveGameListEntryParams
{
    public required UserInformationToken UserInformation { get; set; }
    public required Guid GamesListId { get; set; }
    public required Guid Id { get; set; }
}

public static class RemoveGameListEntryParamsMappingExtensions
{
    public static RemoveGameListEntryParams ToParams(this RemoveGameListEntryRequest request)
    {
        return new RemoveGameListEntryParams
        {
            UserInformation = request.UserInformation,
            GamesListId = request.ListId,
            Id = request.EntryId
        };
    }
}
