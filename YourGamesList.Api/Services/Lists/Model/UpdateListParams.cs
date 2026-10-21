using System;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Requests.Lists;

namespace YourGamesList.Api.Services.Lists.Model;

public class UpdateListParams
{
    public required UserInformationToken UserInformation { get; set; }
    public required Guid Id { get; set; }
    public string? Name { get; set; }
    public bool HasDescription { get; set; }
    public string? Description { get; set; }
    public bool? IsPrivate { get; set; }
}

public static class UpdateListParamsMappingExtensions
{
    public static UpdateListParams ToParams(this UpdateListRequest request)
    {
        var body = request.Body;
        return new UpdateListParams
        {
            UserInformation = request.UserInformation,
            Id = body.Id,
            Name = body.Name.HasValue ? body.Name.Value?.Trim() : null,
            HasDescription = body.Description.HasValue,
            Description = body.Description.HasValue ? body.Description.Value : null,
            IsPrivate = body.IsPrivate.HasValue ? body.IsPrivate.Value : null
        };
    }
}