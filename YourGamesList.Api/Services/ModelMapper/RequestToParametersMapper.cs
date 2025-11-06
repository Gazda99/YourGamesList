using System;
using YourGamesList.Api.Model.Requests.Lists;
using YourGamesList.Api.Services.Ygl.Lists.Model;

namespace YourGamesList.Api.Services.ModelMapper;

public interface IRequestToParametersMapper
{
    UpdateListsParameters Map(UpdateListsRequest request);
    SearchListsParameters Map(SearchListsRequest request);
}

//TODO: unit tests
public class RequestToParametersMapper : IRequestToParametersMapper
{
    public UpdateListsParameters Map(UpdateListsRequest request)
    {
        if (request.Body is null)
        {
            throw new ArgumentNullException(nameof(request.Body));
        }

        return new UpdateListsParameters
        {
            Id = request.Body.Id,
            Name = request.Body.Name,
            Desc = request.Body.Desc,
            IsPublic = request.Body.IsPublic
        };
    }

    public SearchListsParameters Map(SearchListsRequest request)
    {
        if (request.Body is null)
        {
            throw new ArgumentNullException(nameof(request.Body));
        }

        return new SearchListsParameters
        {
            ListName = request.Body.ListName,
            UserName = request.Body.UserName,
            IncludeGames = request.Body.IncludeGames,
            Take = request.Body.Take,
            Skip = request.Body.Skip
        };
    }
}