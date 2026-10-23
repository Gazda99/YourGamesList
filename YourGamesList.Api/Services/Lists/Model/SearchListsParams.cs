using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Lists;

namespace YourGamesList.Api.Services.Lists.Model;

public class SearchListsParams
{
    public required UserInformationToken UserInformation { get; init; }
    public string? OwnerName { get; init; }
    public string? ListName { get; init; }
    public bool IncludeEntries { get; init; }
}

public static class SearchListsParamsMappingExtensions
{
    public static SearchListsParams ToParams(this SearchListsRequest request)
    {
        return new SearchListsParams
        {
            UserInformation = request.UserInformation,
            OwnerName = string.IsNullOrWhiteSpace(request.OwnerName) ? null : request.OwnerName.Trim(),
            ListName = string.IsNullOrWhiteSpace(request.ListName) ? null : request.ListName.Trim(),
            IncludeEntries = request.IncludeEntries
        };
    }
}
