using System;
using System.Linq;
using YourGamesList.Api.Model.Requests.Lists;
using YourGamesList.Api.Services.Ygl.Lists.Model;

namespace YourGamesList.Api.Services.ModelMappers;

public interface IRequestToParametersMapper
{
    UpdateListParameters Map(UpdateListRequest request);
    SearchListsParameters Map(SearchListsRequest request);
    AddEntriesToListParameter Map(AddEntriesToListRequest request);
    DeleteListEntriesParameter Map(DeleteListEntriesRequest request);
    UpdateListEntriesParameter Map(UpdateListEntriesRequest request);
}

public class RequestToParametersMapper : IRequestToParametersMapper
{
    public UpdateListParameters Map(UpdateListRequest request)
    {
        if (request.Body is null)
        {
            throw new ArgumentNullException(nameof(request.Body));
        }

        return new UpdateListParameters
        {
            UserInformation = request.UserInformation,
            ListId = request.Body.ListId,
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

    public AddEntriesToListParameter Map(AddEntriesToListRequest request)
    {
        if (request.Body is null)
        {
            throw new ArgumentNullException(nameof(request.Body));
        }

        return new AddEntriesToListParameter()
        {
            UserInformation = request.UserInformation,
            ListId = request.Body.ListId,
            EntriesToAdd = request.Body.EntriesToAdd.Select(x => new EntryToAddParameter()
            {
                GameId = x.GameId,
                Desc = x.Desc,
                Platforms = x.Platforms,
                GameDistributions = x.GameDistributions,
                IsStarred = x.IsStarred,
                Rating = x.Rating,
                CompletionStatus = x.CompletionStatus
            }).ToArray()
        };
    }

    public DeleteListEntriesParameter Map(DeleteListEntriesRequest request)
    {
        if (request.Body is null)
        {
            throw new ArgumentNullException(nameof(request.Body));
        }

        return new DeleteListEntriesParameter()
        {
            UserInformation = request.UserInformation,
            ListId = request.Body.ListId,
            EntriesToRemove = request.Body.EntriesToRemove
        };
    }

    public UpdateListEntriesParameter Map(UpdateListEntriesRequest request)
    {
        if (request.Body is null)
        {
            throw new ArgumentNullException(nameof(request.Body));
        }
        
        return new UpdateListEntriesParameter()
        {
            UserInformation = request.UserInformation,
            ListId = request.Body.ListId,
            EntriesToUpdate = request.Body.EntriesToUpdate.Select(x => new EntryToUpdateParameter()
            {
                EntryId = x.EntryId,
                Desc = x.Desc,
                Platforms = x.Platforms,
                GameDistributions = x.GameDistributions,
                IsStarred = x.IsStarred,
                Rating = x.Rating,
                CompletionStatus = x.CompletionStatus
            }).ToArray()
        };
    }
}