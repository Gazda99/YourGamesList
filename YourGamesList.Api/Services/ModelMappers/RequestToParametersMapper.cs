using System;
using System.Linq;
using YourGamesList.Api.Model.Requests.Lists;
using YourGamesList.Api.Model.Requests.Users;
using YourGamesList.Api.Services.Users.Model;
using YourGamesList.Api.Services.Ygl.Lists.Model;

namespace YourGamesList.Api.Services.ModelMappers;

public interface IRequestToParametersMapper
{
    CreateListParameters Map(CreateListRequest request);
    UpdateListParameters Map(UpdateListRequest request);
    SearchListsParameters Map(SearchListsRequest request);
    AddEntriesToListParameter Map(AddEntriesToListRequest request);
    DeleteListEntriesParameter Map(DeleteListEntriesRequest request);
    UpdateListEntriesParameter Map(UpdateListEntriesRequest request);
    UserUpdateParameters Map(UserUpdateRequest request);
    UserGetSelfParameters Map(UserGetSelfRequest request);
    AddOwnershipInfoToEntryParameters Map(AddOwnershipInfoToEntryRequest request);
    DeleteOwnershipInfoToEntryParameters Map(DeleteOwnershipInfoToEntryRequest request);
}

public class RequestToParametersMapper : IRequestToParametersMapper
{
    public CreateListParameters Map(CreateListRequest request)
    {
        if (request.Body is null)
        {
            throw new ArgumentNullException(nameof(request.Body));
        }

        return new CreateListParameters
        {
            UserInformation = request.UserInformation,
            ListName = request.Body.ListName,
            Description = request.Body.Description,
            IsPublic = request.Body.IsPublic
        };
    }
    
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
            Description = request.Body.Description,
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
                Description = x.Description,
                // Platforms = x.Platforms,
                // GameDistributions = x.GameDistributions,
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
                Description = x.Description,
                // Platforms = x.Platforms,
                // GameDistributions = x.GameDistributions,
                IsStarred = x.IsStarred,
                Rating = x.Rating,
                CompletionStatus = x.CompletionStatus
            }).ToArray()
        };
    }

    public UserUpdateParameters Map(UserUpdateRequest request)
    {
        if (request.Body is null)
        {
            throw new ArgumentNullException(nameof(request.Body));
        }

        return new UserUpdateParameters()
        {
            UserInformation = request.UserInformation,
            Country = request.Body.Country,
            Description = request.Body.Description,
            DateOfBirth = request.Body.DateOfBirth,
        };
    }

    public UserGetSelfParameters Map(UserGetSelfRequest request)
    {
        return new UserGetSelfParameters()
        {
            UserInformation = request.UserInformation
        };
    }

    public AddOwnershipInfoToEntryParameters Map(AddOwnershipInfoToEntryRequest request)
    {
        if (request.Body is null)
        {
            throw new ArgumentNullException(nameof(request.Body));
        }

        return new AddOwnershipInfoToEntryParameters()
        {
            UserInformation = request.UserInformation,
            ListEntryId = request.Body.ListEntryId,
            OwnershipsToAdd = request.Body.OwnershipsToAdd.Select(x => new OwnershipsToAddParameter()
            {
                Platform = x.Platform,
                GameDistribution = x.GameDistribution,
                IsLegit = x.IsLegit,
                WasEmulated = x.WasEmulated,
                EmulatedOn = x.EmulatedOn
            }).ToArray()
        };
    }

    public DeleteOwnershipInfoToEntryParameters Map(DeleteOwnershipInfoToEntryRequest request)
    {
        if (request.Body is null)
        {
            throw new ArgumentNullException(nameof(request.Body));
        }

        return new DeleteOwnershipInfoToEntryParameters()
        {
            UserInformation = request.UserInformation,
            ListEntryId = request.Body.ListEntryId,
            OwnershipsToRemove = request.Body.OwnershipsToRemove
        };
    }
}