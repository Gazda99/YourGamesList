using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Services.Lists.Model;
using YourGamesList.Common;
using YourGamesList.Contracts.Dto;
using YourGamesList.Database;
using YourGamesList.Database.Entity;

namespace YourGamesList.Api.Services.Lists;

public interface IListsManagerService
{
    /// <summary>
    /// Gets the current user's lists.
    /// </summary>
    Task<CombinedResult<List<GamesListDto>, ListsError>> GetSelf(GetSelfListParams getSelfListParams);

    /// <summary>
    /// Creates a list for the current user.
    /// </summary>
    Task<CombinedResult<Guid, ListsError>> Create(CreateListParams createListParams);

    /// <summary>
    /// Deletes a list owned by the current user.
    /// </summary>
    Task<ErrorResult<ListsError>> Delete(DeleteListParams deleteListParams);

    /// <summary>
    /// Updates metadata on a list owned by the current user.
    /// </summary>
    Task<ErrorResult<ListsError>> Update(UpdateListParams updateListParams);
}

public class ListsManagerService : IListsManagerService
{
    private readonly ILogger<ListsManagerService> _logger;
    private readonly YourGamesListDbContext _yglDbContext;
    private readonly TimeProvider _timeProvider;

    public ListsManagerService(
        ILogger<ListsManagerService> logger,
        YourGamesListDbContext yglDbContext,
        TimeProvider timeProvider)
    {
        _logger = logger;
        _yglDbContext = yglDbContext;
        _timeProvider = timeProvider;
    }

    public async Task<CombinedResult<List<GamesListDto>, ListsError>> GetSelf(GetSelfListParams getSelfListParams)
    {
        var userId = getSelfListParams.UserInformation.UserId;

        var user = await _yglDbContext.Users
            .AsNoTracking()
            .AsSplitQuery()
            .Include(user => user.GamesLists)
            .ThenInclude(list => list.GamesListEntries)
            .ThenInclude(listEntry => listEntry.OwnershipInfo)
            .FirstOrDefaultAsync(x => x.Id == userId);

        if (user == null)
        {
            _logger.LogInformation("Cannot find user.");
            return CombinedResult<List<GamesListDto>, ListsError>.Failure(ListsError.UserNotFound);
        }

        var gamesLists = user.GamesLists.Select(list => new GamesListDto
        {
            CreatedAt = list.CreatedAt,
            Description = list.Description,
            Id = list.Id,
            IsPrivate = list.IsPrivate,
            Name = list.Name,
            GameListEntries = list.GamesListEntries.Select(entry => new GamesListEntryDto
            {
                CompletionStatus = (CompletionStatusDto)entry.CompletionStatus,
                Description = entry.Description,
                GamesListId = entry.GamesListId,
                IgdbId = entry.IgdbId,
                Rating = entry.Rating,
                OwnershipInfos = entry.OwnershipInfo.Select(ownership => new OwnershipInfoDto
                {
                    IsLegit = ownership.IsLegit,
                    Platform = (PlatformDto)ownership.Platform,
                    GameDistribution = (GameDistributionDto)ownership.GameDistribution,
                    WasEmulated = ownership.WasEmulated,
                    EmulatedOn = ownership.EmulatedOn.HasValue
                        ? (EmulatorDto)ownership.EmulatedOn.Value
                        : null
                }).ToList()
            }).ToList()
        }).ToList();
        return CombinedResult<List<GamesListDto>, ListsError>.Success(gamesLists);
    }

    public async Task<CombinedResult<Guid, ListsError>> Create(CreateListParams createListParams)
    {
        var userId = createListParams.UserInformation.UserId;
        var user = await _yglDbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
        {
            _logger.LogInformation("Cannot find user.");
            return CombinedResult<Guid, ListsError>.Failure(ListsError.UserNotFound);
        }

        var listAlreadyExists = await _yglDbContext.GamesLists
            .AnyAsync(list => list.UserId == userId && list.Name == createListParams.Name);
        if (listAlreadyExists)
        {
            _logger.LogInformation("User already has a list named {ListName}.", createListParams.Name);
            return CombinedResult<Guid, ListsError>.Failure(ListsError.ListAlreadyExists);
        }

        var gamesList = new GamesList
        {
            CreatedAt = _timeProvider.GetUtcNow(),
            Editable = true,
            IsPrivate = true,
            Name = createListParams.Name,
            User = user
        };

        _yglDbContext.GamesLists.Add(gamesList);
        await _yglDbContext.SaveChangesAsync();

        return CombinedResult<Guid, ListsError>.Success(gamesList.Id);
    }

    public async Task<ErrorResult<ListsError>> Delete(DeleteListParams deleteListParams)
    {
        var userId = deleteListParams.UserInformation.UserId;
        var gamesList = await _yglDbContext.GamesLists
            .FirstOrDefaultAsync(list => list.Id == deleteListParams.Id && list.UserId == userId);

        if (gamesList == null)
        {
            _logger.LogInformation("Cannot find list {GamesListId} for the requesting user.", deleteListParams.Id);
            return ErrorResult<ListsError>.Failure(ListsError.ListNotFound);
        }

        if (!gamesList.Editable)
        {
            _logger.LogInformation("Cannot delete protected list {GamesListId}.", deleteListParams.Id);
            return ErrorResult<ListsError>.Failure(ListsError.ListCannotBeDeleted);
        }

        _yglDbContext.GamesLists.Remove(gamesList);
        await _yglDbContext.SaveChangesAsync();

        return ErrorResult<ListsError>.Success();
    }

    public async Task<ErrorResult<ListsError>> Update(UpdateListParams updateListParams)
    {
        var userId = updateListParams.UserInformation.UserId;
        var gamesList = await _yglDbContext.GamesLists
            .FirstOrDefaultAsync(list => list.Id == updateListParams.Id && list.UserId == userId);

        if (gamesList == null)
        {
            _logger.LogInformation("Cannot find list {GamesListId} for the requesting user.", updateListParams.Id);
            return ErrorResult<ListsError>.Failure(ListsError.ListNotFound);
        }

        var nameChanged = updateListParams.Name != null && updateListParams.Name != gamesList.Name;
        var descriptionChanged = updateListParams.HasDescription &&
                                 updateListParams.Description != gamesList.Description;
        var privacyChanged = updateListParams.IsPrivate.HasValue &&
                             updateListParams.IsPrivate.Value != gamesList.IsPrivate;

        if (!nameChanged && !descriptionChanged && !privacyChanged)
        {
            return ErrorResult<ListsError>.Success();
        }

        if (!gamesList.Editable)
        {
            _logger.LogInformation("Cannot edit protected list {GamesListId}.", updateListParams.Id);
            return ErrorResult<ListsError>.Failure(ListsError.ListCannotBeEdited);
        }

        if (nameChanged)
        {
            var listAlreadyExists = await _yglDbContext.GamesLists
                .AnyAsync(list =>
                    list.UserId == userId &&
                    list.Id != gamesList.Id &&
                    list.Name == updateListParams.Name);

            if (listAlreadyExists)
            {
                _logger.LogInformation("User already has a list named {ListName}.", updateListParams.Name);
                return ErrorResult<ListsError>.Failure(ListsError.ListAlreadyExists);
            }

            gamesList.Name = updateListParams.Name!;
        }

        if (descriptionChanged)
        {
            gamesList.Description = updateListParams.Description;
        }

        if (privacyChanged)
        {
            gamesList.IsPrivate = updateListParams.IsPrivate!.Value;
        }

        gamesList.ModifiedAt = _timeProvider.GetUtcNow();
        await _yglDbContext.SaveChangesAsync();

        return ErrorResult<ListsError>.Success();
    }

}