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
    #region Lists

    /// <summary>
    /// Gets the current user's lists.
    /// </summary>
    Task<CombinedResult<List<GamesListDto>, ListsError>> GetSelf(GetSelfListParams getSelfListParams);

    /// <summary>
    /// Searches visible lists by owner username or list name.
    /// </summary>
    Task<List<GamesListDto>> Search(SearchListsParams searchListsParams);

    /// <summary>
    /// Creates a list for the current user.
    /// </summary>
    Task<CombinedResult<Guid, ListsError>> Create(AddListParams addListParams);

    /// <summary>
    /// Deletes a list owned by the current user.
    /// </summary>
    Task<ErrorResult<ListsError>> Delete(DeleteListParams deleteListParams);

    /// <summary>
    /// Updates metadata on a list owned by the current user.
    /// </summary>
    Task<ErrorResult<ListsError>> Update(UpdateListParams updateListParams);

    #endregion

    #region Game list entries

    /// <summary>
    /// Adds a game to a list owned by the current user.
    /// </summary>
    Task<CombinedResult<Guid, ListsError>> AddGameListEntry(AddGameListEntryParams addParams);

    /// <summary>
    /// Removes a game from a list owned by the current user.
    /// </summary>
    Task<ErrorResult<ListsError>> RemoveGameListEntry(RemoveGameListEntryParams removeParams);

    /// <summary>
    /// Updates a game entry in a list owned by the current user.
    /// </summary>
    Task<ErrorResult<ListsError>> UpdateGameListEntry(UpdateGameListEntryParams updateParams);

    /// <summary>
    /// Adds ownership info to a game entry in a list owned by the current user.
    /// </summary>
    Task<CombinedResult<Guid, ListsError>> AddOwnershipInfo(AddOwnershipInfoParams addParams);

    /// <summary>
    /// Updates ownership info on a game entry in a list owned by the current user.
    /// </summary>
    Task<ErrorResult<ListsError>> UpdateOwnershipInfo(UpdateOwnershipInfoParams updateParams);

    /// <summary>
    /// Removes ownership info from a game entry in a list owned by the current user.
    /// </summary>
    Task<ErrorResult<ListsError>> RemoveOwnershipInfo(RemoveOwnershipInfoParams removeParams);

    #endregion
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

    #region Lists

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

        var gamesLists = user.GamesLists.Select(list => MapGamesList(list, includeEntries: true)).ToList();
        
        return CombinedResult<List<GamesListDto>, ListsError>.Success(gamesLists);
    }

    public async Task<List<GamesListDto>> Search(SearchListsParams searchListsParams)
    {
        var ownerName = searchListsParams.OwnerName?.ToLowerInvariant();
        var listName = searchListsParams.ListName?.ToLowerInvariant();
        var gamesListsQuery = _yglDbContext.GamesLists
            .AsNoTracking()
            .Where(list => !list.IsPrivate || list.UserId == searchListsParams.UserInformation.UserId);

        if (ownerName != null)
        {
            gamesListsQuery = gamesListsQuery.Where(list => list.User.NormalizedUsername.Contains(ownerName));
        }

        if (listName != null)
        {
            gamesListsQuery = gamesListsQuery.Where(list => list.NormalizedName.Contains(listName));
        }

        if (searchListsParams.IncludeEntries)
        {
            gamesListsQuery = gamesListsQuery
                .AsSplitQuery()
                .Include(list => list.GamesListEntries)
                .ThenInclude(entry => entry.OwnershipInfo);
        }

        var gamesLists = await gamesListsQuery.ToListAsync();
        return gamesLists.Select(list => MapGamesList(list, searchListsParams.IncludeEntries)).ToList();
    }

    private static GamesListDto MapGamesList(GamesList list, bool includeEntries)
    {
        return new GamesListDto
        {
            CreatedAt = list.CreatedAt,
            Description = list.Description,
            Id = list.Id,
            IsPrivate = list.IsPrivate,
            Name = list.Name,
            GameListEntries = includeEntries
                ? list.GamesListEntries.Select(entry => new GamesListEntryDto
                {
                    CompletionStatus = (CompletionStatusDto)entry.CompletionStatus,
                    Description = entry.Description,
                    Id = entry.Id,
                    GamesListId = entry.GamesListId,
                    IgdbId = entry.IgdbId,
                    Rating = entry.Rating,
                    OwnershipInfos = entry.OwnershipInfo.Select(ownership => new OwnershipInfoDto
                    {
                        Id = ownership.Id,
                        IsLegit = ownership.IsLegit,
                        Platform = (PlatformDto)ownership.Platform,
                        GameDistribution = (GameDistributionDto)ownership.GameDistribution,
                        WasEmulated = ownership.WasEmulated,
                        EmulatedOn = ownership.EmulatedOn.HasValue
                            ? (EmulatorDto)ownership.EmulatedOn.Value
                            : null
                    }).ToList()
                }).ToList()
                : []
        };
    }

    public async Task<CombinedResult<Guid, ListsError>> Create(AddListParams addListParams)
    {
        var userId = addListParams.UserInformation.UserId;
        var user = await _yglDbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
        {
            _logger.LogInformation("Cannot find user.");
            return CombinedResult<Guid, ListsError>.Failure(ListsError.UserNotFound);
        }

        var normalizedName = addListParams.Name.ToLowerInvariant();
        var listAlreadyExists = await _yglDbContext.GamesLists
            .AnyAsync(list => list.UserId == userId && list.NormalizedName == normalizedName);
        if (listAlreadyExists)
        {
            _logger.LogInformation("User already has a list named {ListName}.", addListParams.Name);
            return CombinedResult<Guid, ListsError>.Failure(ListsError.ListAlreadyExists);
        }

        var gamesList = new GamesList
        {
            CreatedAt = _timeProvider.GetUtcNow(),
            Editable = true,
            IsPrivate = true,
            Name = addListParams.Name,
            NormalizedName = normalizedName,
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
            _logger.LogInformation("Cannot remove protected list {GamesListId}.", deleteListParams.Id);
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
            var normalizedName = updateListParams.Name!.ToLowerInvariant();
            var listAlreadyExists = await _yglDbContext.GamesLists
                .AnyAsync(list =>
                    list.UserId == userId &&
                    list.Id != gamesList.Id &&
                    list.NormalizedName == normalizedName);

            if (listAlreadyExists)
            {
                _logger.LogInformation("User already has a list named {ListName}.", updateListParams.Name);
                return ErrorResult<ListsError>.Failure(ListsError.ListAlreadyExists);
            }

            gamesList.Name = updateListParams.Name!;
            gamesList.NormalizedName = normalizedName;
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

    #endregion

    #region Game list entries

    public async Task<CombinedResult<Guid, ListsError>> AddGameListEntry(AddGameListEntryParams addParams)
    {
        var userId = addParams.UserInformation.UserId;
        var gamesList = await _yglDbContext.GamesLists.FirstOrDefaultAsync(list => list.Id == addParams.GamesListId && list.UserId == userId);

        if (gamesList == null)
        {
            _logger.LogInformation("Cannot find list {GamesListId} for the requesting user.", addParams.GamesListId);
            return CombinedResult<Guid, ListsError>.Failure(ListsError.ListNotFound);
        }

        var entryAlreadyExists = await _yglDbContext.GameListEntries.AnyAsync(entry => entry.GamesListId == gamesList.Id && entry.IgdbId == addParams.IgdbId);

        if (entryAlreadyExists)
        {
            _logger.LogInformation("Game {IgdbId} already exists in list {GamesListId}.", addParams.IgdbId, gamesList.Id);
            return CombinedResult<Guid, ListsError>.Failure(ListsError.GameListEntryAlreadyExists);
        }

        var entry = new GamesListEntry
        {
            Id = Guid.NewGuid(),
            CreatedAt = _timeProvider.GetUtcNow(),
            CompletionStatus = (CompletionStatus)addParams.CompletionStatus,
            Description = addParams.Description,
            IgdbId = addParams.IgdbId,
            Rating = addParams.Rating,
            GamesListId = gamesList.Id
        };

        _yglDbContext.GameListEntries.Add(entry);

        await _yglDbContext.SaveChangesAsync();

        return CombinedResult<Guid, ListsError>.Success(entry.Id);
    }

    public async Task<ErrorResult<ListsError>> RemoveGameListEntry(RemoveGameListEntryParams removeParams)
    {
        var userId = removeParams.UserInformation.UserId;
        var entry = await _yglDbContext.GameListEntries
            .FirstOrDefaultAsync(gameListEntry =>
                gameListEntry.Id == removeParams.Id &&
                gameListEntry.GamesListId == removeParams.GamesListId &&
                gameListEntry.GamesList.UserId == userId);

        if (entry == null)
        {
            _logger.LogInformation("Cannot find game list entry {EntryId} for the requesting user.", removeParams.Id);
            return ErrorResult<ListsError>.Failure(ListsError.GameListEntryNotFound);
        }

        _yglDbContext.GameListEntries.Remove(entry);
        await _yglDbContext.SaveChangesAsync();
        return ErrorResult<ListsError>.Success();
    }

    public async Task<ErrorResult<ListsError>> UpdateGameListEntry(UpdateGameListEntryParams updateParams)
    {
        var userId = updateParams.UserInformation.UserId;
        var entry = await _yglDbContext.GameListEntries
            .Include(gameListEntry => gameListEntry.GamesList)
            .FirstOrDefaultAsync(gameListEntry =>
                gameListEntry.Id == updateParams.Id &&
                gameListEntry.GamesListId == updateParams.GamesListId &&
                gameListEntry.GamesList.UserId == userId);

        if (entry == null)
        {
            _logger.LogInformation("Cannot find game list entry {EntryId} for the requesting user.", updateParams.Id);
            return ErrorResult<ListsError>.Failure(ListsError.GameListEntryNotFound);
        }

        var completionStatusChanged = updateParams.CompletionStatus.HasValue &&
                                      updateParams.CompletionStatus.Value != (CompletionStatusDto)entry.CompletionStatus;
        var descriptionChanged = updateParams.Description.HasValue &&
                                 updateParams.Description.Value != entry.Description;
        var ratingChanged = updateParams.Rating.HasValue &&
                            updateParams.Rating.Value != entry.Rating;

        if (!completionStatusChanged && !descriptionChanged && !ratingChanged)
        {
            return ErrorResult<ListsError>.Success();
        }

        if (!entry.GamesList.Editable)
        {
            _logger.LogInformation("Cannot edit game list entry {EntryId} in a protected list.", updateParams.Id);
            return ErrorResult<ListsError>.Failure(ListsError.ListCannotBeEdited);
        }

        if (completionStatusChanged)
        {
            entry.CompletionStatus = (CompletionStatus)updateParams.CompletionStatus.Value!.Value;
        }

        if (descriptionChanged)
        {
            entry.Description = updateParams.Description.Value;
        }

        if (ratingChanged)
        {
            entry.Rating = updateParams.Rating.Value;
        }

        entry.ModifiedAt = _timeProvider.GetUtcNow();
        await _yglDbContext.SaveChangesAsync();
        return ErrorResult<ListsError>.Success();
    }

    public async Task<CombinedResult<Guid, ListsError>> AddOwnershipInfo(AddOwnershipInfoParams addParams)
    {
        var entryExists = await _yglDbContext.GameListEntries.AnyAsync(entry =>
            entry.Id == addParams.GameListEntryId &&
            entry.GamesListId == addParams.GamesListId &&
            entry.GamesList.UserId == addParams.UserInformation.UserId);

        if (!entryExists)
        {
            _logger.LogInformation(
                "Cannot find game list entry {EntryId} in list {GamesListId} for the requesting user.",
                addParams.GameListEntryId,
                addParams.GamesListId);
            return CombinedResult<Guid, ListsError>.Failure(ListsError.GameListEntryNotFound);
        }

        var ownershipInfo = new OwnershipInfo
        {
            Id = Guid.NewGuid(),
            CreatedAt = _timeProvider.GetUtcNow(),
            IsLegit = addParams.IsLegit,
            Platform = (Platform)addParams.Platform,
            GameDistribution = (GameDistribution)addParams.GameDistribution,
            WasEmulated = addParams.WasEmulated,
            EmulatedOn = addParams.EmulatedOn.HasValue
                ? (Emulator)addParams.EmulatedOn.Value
                : null,
            GameListEntryId = addParams.GameListEntryId
        };

        _yglDbContext.OwnershipInfos.Add(ownershipInfo);
        await _yglDbContext.SaveChangesAsync();

        return CombinedResult<Guid, ListsError>.Success(ownershipInfo.Id);
    }

    public async Task<ErrorResult<ListsError>> UpdateOwnershipInfo(UpdateOwnershipInfoParams updateParams)
    {
        var entryExists = await _yglDbContext.GameListEntries.AnyAsync(entry =>
            entry.Id == updateParams.GameListEntryId &&
            entry.GamesListId == updateParams.GamesListId &&
            entry.GamesList.UserId == updateParams.UserInformation.UserId);

        if (!entryExists)
        {
            _logger.LogInformation(
                "Cannot find game list entry {EntryId} in list {GamesListId} for the requesting user.",
                updateParams.GameListEntryId,
                updateParams.GamesListId);
            return ErrorResult<ListsError>.Failure(ListsError.GameListEntryNotFound);
        }

        var ownershipInfo = await _yglDbContext.OwnershipInfos.FirstOrDefaultAsync(info =>
            info.Id == updateParams.OwnershipInfoId &&
            info.GameListEntryId == updateParams.GameListEntryId);

        if (ownershipInfo == null)
        {
            _logger.LogInformation("Cannot find ownership info {OwnershipInfoId}.", updateParams.OwnershipInfoId);
            return ErrorResult<ListsError>.Failure(ListsError.OwnershipInfoNotFound);
        }

        var isLegitChanged = updateParams.IsLegit.HasValue &&
                             updateParams.IsLegit.Value != ownershipInfo.IsLegit;
        var platformChanged = updateParams.Platform.HasValue &&
                              updateParams.Platform.Value != (PlatformDto)ownershipInfo.Platform;
        var gameDistributionChanged = updateParams.GameDistribution.HasValue &&
                                      updateParams.GameDistribution.Value !=
                                      (GameDistributionDto)ownershipInfo.GameDistribution;
        var wasEmulatedChanged = updateParams.WasEmulated.HasValue &&
                                 updateParams.WasEmulated.Value != ownershipInfo.WasEmulated;
        var emulatedOnChanged = updateParams.EmulatedOn.HasValue &&
                                updateParams.EmulatedOn.Value !=
                                (EmulatorDto?)ownershipInfo.EmulatedOn;

        if (!isLegitChanged &&
            !platformChanged &&
            !gameDistributionChanged &&
            !wasEmulatedChanged &&
            !emulatedOnChanged)
        {
            return ErrorResult<ListsError>.Success();
        }

        if (isLegitChanged)
        {
            ownershipInfo.IsLegit = updateParams.IsLegit.Value;
        }

        if (platformChanged)
        {
            ownershipInfo.Platform = (Platform)updateParams.Platform.Value!.Value;
        }

        if (gameDistributionChanged)
        {
            ownershipInfo.GameDistribution = (GameDistribution)updateParams.GameDistribution.Value!.Value;
        }

        if (wasEmulatedChanged)
        {
            ownershipInfo.WasEmulated = updateParams.WasEmulated.Value!.Value;
        }

        if (emulatedOnChanged)
        {
            ownershipInfo.EmulatedOn = updateParams.EmulatedOn.Value.HasValue
                ? (Emulator)updateParams.EmulatedOn.Value.Value
                : null;
        }

        ownershipInfo.ModifiedAt = _timeProvider.GetUtcNow();
        await _yglDbContext.SaveChangesAsync();
        return ErrorResult<ListsError>.Success();
    }

    public async Task<ErrorResult<ListsError>> RemoveOwnershipInfo(RemoveOwnershipInfoParams removeParams)
    {
        var entryExists = await _yglDbContext.GameListEntries.AnyAsync(entry =>
            entry.Id == removeParams.GameListEntryId &&
            entry.GamesListId == removeParams.GamesListId &&
            entry.GamesList.UserId == removeParams.UserInformation.UserId);

        if (!entryExists)
        {
            _logger.LogInformation(
                "Cannot find game list entry {EntryId} in list {GamesListId} for the requesting user.",
                removeParams.GameListEntryId,
                removeParams.GamesListId);
            return ErrorResult<ListsError>.Failure(ListsError.GameListEntryNotFound);
        }

        var ownershipInfo = await _yglDbContext.OwnershipInfos.FirstOrDefaultAsync(info =>
            info.Id == removeParams.OwnershipInfoId &&
            info.GameListEntryId == removeParams.GameListEntryId);

        if (ownershipInfo == null)
        {
            _logger.LogInformation("Cannot find ownership info {OwnershipInfoId}.", removeParams.OwnershipInfoId);
            return ErrorResult<ListsError>.Failure(ListsError.OwnershipInfoNotFound);
        }

        _yglDbContext.OwnershipInfos.Remove(ownershipInfo);
        await _yglDbContext.SaveChangesAsync();
        return ErrorResult<ListsError>.Success();
    }

    #endregion
}