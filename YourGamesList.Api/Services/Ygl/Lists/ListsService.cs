using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Model;
using YourGamesList.Api.Services.ModelMappers;
using YourGamesList.Api.Services.Ygl.Lists.Model;
using YourGamesList.Common;
using YourGamesList.Contracts.Dto;
using YourGamesList.Database;
using YourGamesList.Database.Entities;

namespace YourGamesList.Api.Services.Ygl.Lists;

public interface IListsService
{
    #region List

    Task<CombinedResult<Guid, ListsError>> CreateList(JwtUserInformation userInfo, string listName, string? description);
    Task<CombinedResult<GamesListDto, ListsError>> GetList(JwtUserInformation userInfo, Guid listId, bool includeGames);
    Task<CombinedResult<List<GamesListDto>, ListsError>> SearchLists(SearchListsParameters parameters);
    Task<CombinedResult<List<GamesListDto>, ListsError>> GetSelfLists(JwtUserInformation userInfo, bool includeGames);
    Task<CombinedResult<Guid, ListsError>> UpdateList(UpdateListParameters parameters);
    Task<CombinedResult<Guid, ListsError>> DeleteList(JwtUserInformation userInfo, Guid listId);

    #endregion

    #region ListEntry

    Task<CombinedResult<List<Guid>, ListsError>> AddListEntries(AddEntriesToListParameter parameters);
    Task<CombinedResult<List<Guid>, ListsError>> DeleteListEntries(DeleteListEntriesParameter parameters);
    Task<CombinedResult<List<Guid>, ListsError>> UpdateListEntries(UpdateListEntriesParameter parameters);

    #endregion
}

public class ListsService : IListsService
{
    private readonly ILogger<ListsService> _logger;
    private readonly IYglDatabaseAndDtoMapper _yglDatabaseAndDtoMapper;
    private readonly TimeProvider _timeProvider;
    private readonly YglDbContext _yglDbContext;

    public ListsService(
        ILogger<ListsService> logger,
        IDbContextFactory<YglDbContext> yglDbContext,
        IYglDatabaseAndDtoMapper yglDatabaseAndDtoMapper,
        TimeProvider timeProvider
    )
    {
        _logger = logger;
        _yglDatabaseAndDtoMapper = yglDatabaseAndDtoMapper;
        _timeProvider = timeProvider;
        _yglDbContext = yglDbContext.CreateDbContext();
    }

    #region List

    public async Task<CombinedResult<Guid, ListsError>> CreateList(JwtUserInformation userInfo, string listName, string? description = null)
    {
        var listsQuery = await _yglDbContext.Lists.FirstOrDefaultAsync(x => x.UserId == userInfo.UserId && x.Name.ToLower() == listName.ToLower());
        if (listsQuery != null)
        {
            _logger.LogInformation($"'{listName}' already exists.");
            return CombinedResult<Guid, ListsError>.Failure(ListsError.ListAlreadyExists);
        }

        var list = new GamesList()
        {
            Name = listName,
            UserId = userInfo.UserId,
            Desc = string.IsNullOrEmpty(description) ? string.Empty : description,
            IsPublic = true,
            CanBeDeleted = true,
            CreatedDate = _timeProvider.GetUtcNow()
        };

        await _yglDbContext.Lists.AddAsync(list);
        await _yglDbContext.SaveChangesAsync();
        _logger.LogInformation($"Created new list with id '{list.Id}' for user '{userInfo.UserId}'.");
        return CombinedResult<Guid, ListsError>.Success(list.Id);
    }

    public async Task<CombinedResult<GamesListDto, ListsError>> GetList(JwtUserInformation userInfo, Guid listId, bool includeGames)
    {
        var listsQuery = _yglDbContext.Lists.AsNoTracking();

        if (includeGames)
        {
            listsQuery = listsQuery.Include(x => x.Entries).ThenInclude(x => x.Game);
        }
        else
        {
            listsQuery = listsQuery.Include(x => x.Entries);
        }

        var list = await listsQuery.FirstOrDefaultAsync(x => x.Id == listId);

        if (list == null)
        {
            _logger.LogInformation($"No lists found with id '{listId}'.");
            return CombinedResult<GamesListDto, ListsError>.Failure(ListsError.ListNotFound);
        }

        //If list is public, we need to verify if the user making the request is the user owning that list
        if (list.IsPublic == false)
        {
            if (list.UserId != userInfo.UserId)
            {
                _logger.LogInformation($"List with id '{listId}' found, but is not public and does not belong to the user making the request.");
                return CombinedResult<GamesListDto, ListsError>.Failure(ListsError.ForbiddenList);
            }
        }

        var listDto = _yglDatabaseAndDtoMapper.Map(list);

        _logger.LogInformation($"Found list with id {listId}.");
        return CombinedResult<GamesListDto, ListsError>.Success(listDto);
    }

    public async Task<CombinedResult<List<GamesListDto>, ListsError>> SearchLists(SearchListsParameters parameters)
    {
        //Verify at least one search parameter is provided
        if (string.IsNullOrWhiteSpace(parameters.ListName) && string.IsNullOrWhiteSpace(parameters.UserName))
        {
            _logger.LogInformation("No search parameters provided.");
            return CombinedResult<List<GamesListDto>, ListsError>.Failure(ListsError.BadRequest);
        }

        var listsQuery = _yglDbContext.Lists.AsNoTracking();
        listsQuery = listsQuery.Include(x => x.User);
        listsQuery = listsQuery.Where(x => x.IsPublic == true);

        //List name first
        if (!string.IsNullOrWhiteSpace(parameters.ListName))
        {
            listsQuery = listsQuery.Where(x => x.Name.ToLower().Contains(parameters.ListName.ToLower()));
        }
        //Then userName
        else if (!string.IsNullOrWhiteSpace(parameters.UserName))
        {
            //TODO: maybe use contains?
            listsQuery = listsQuery.Where(x => x.User.Username.ToLower() == parameters.UserName.ToLower());
        }

        if (parameters.IncludeGames)
        {
            listsQuery = listsQuery.Include(x => x.Entries).ThenInclude(x => x.Game);
        }
        else
        {
            listsQuery = listsQuery.Include(x => x.Entries);
        }

        listsQuery = listsQuery.Skip(parameters.Skip);
        listsQuery = listsQuery.Take(parameters.Take);

        var lists = await listsQuery.ToListAsync();

        if (lists.Count == 0)
        {
            _logger.LogInformation("No lists found for the provided search parameters.");
            return CombinedResult<List<GamesListDto>, ListsError>.Failure(ListsError.ListNotFound);
        }

        var listDtos = GamesListToDto(lists);
        if (!string.IsNullOrWhiteSpace(parameters.ListName))
        {
            listDtos = listDtos.OrderBy(x => x.Name.StartsWith(parameters.ListName, StringComparison.CurrentCultureIgnoreCase)).ToList();
        }

        _logger.LogInformation($"Found '{listDtos.Count}' lists.");
        return CombinedResult<List<GamesListDto>, ListsError>.Success(listDtos);
    }

    public async Task<CombinedResult<List<GamesListDto>, ListsError>> GetSelfLists(JwtUserInformation userInfo, bool includeGames)
    {
        var listsQuery = _yglDbContext.Lists.AsNoTracking();

        if (includeGames)
        {
            listsQuery = listsQuery.Include(x => x.Entries).ThenInclude(x => x.Game);
        }
        else
        {
            listsQuery = listsQuery.Include(x => x.Entries);
        }

        listsQuery = listsQuery.Where(x => x.UserId == userInfo.UserId);

        var lists = await listsQuery.ToListAsync();

        if (lists.Count == 0)
        {
            _logger.LogInformation($"No lists found for the user '{userInfo.UserId}'.");
            return CombinedResult<List<GamesListDto>, ListsError>.Failure(ListsError.ListNotFound);
        }

        var listDtos = GamesListToDto(lists);

        _logger.LogInformation($"Found '{listDtos.Count}' lists for the user '{userInfo.UserId}'.");
        return CombinedResult<List<GamesListDto>, ListsError>.Success(listDtos);
    }

    public async Task<CombinedResult<Guid, ListsError>> UpdateList(UpdateListParameters parameters)
    {
        var list = await _yglDbContext.Lists.FirstOrDefaultAsync(x => x.UserId == parameters.UserInformation.UserId && x.Id == parameters.ListId);
        if (list == null)
        {
            _logger.LogInformation($"No lists found for id '{parameters.ListId}' and user '{parameters.UserInformation.UserId}'.");
            return CombinedResult<Guid, ListsError>.Failure(ListsError.ListNotFound);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Name) && !list.Name.Equals(parameters.Name, StringComparison.CurrentCultureIgnoreCase))
        {
            var nameExists =
                await _yglDbContext.Lists.AnyAsync(x => x.UserId == list.UserId && x.Id != list.Id && x.Name.ToLower() == parameters.Name.ToLower());
            if (nameExists)
            {
                _logger.LogInformation($"Cannot rename list to '{parameters.Name}' because it already exists for user '{list.UserId}'.");
                return CombinedResult<Guid, ListsError>.Failure(ListsError.ListAlreadyExists);
            }

            list.Name = parameters.Name;
        }

        if (!string.IsNullOrWhiteSpace(parameters.Desc))
        {
            list.Desc = parameters.Desc;
        }

        if (parameters.IsPublic != null)
        {
            list.IsPublic = parameters.IsPublic.Value;
        }

        list.LastModifiedDate = _timeProvider.GetUtcNow();
        await _yglDbContext.SaveChangesAsync();

        _logger.LogInformation("List updated successfully.");
        return CombinedResult<Guid, ListsError>.Success(list.Id);
    }

    public async Task<CombinedResult<Guid, ListsError>> DeleteList(JwtUserInformation userInfo, Guid listId)
    {
        _logger.LogInformation($"Search for list '{listId}' which belongs to user '{userInfo.Username}'.");
        var list = await _yglDbContext.Lists.FirstOrDefaultAsync(x =>
            x.UserId == userInfo.UserId &&
            x.Id == listId
        );

        if (list == null)
        {
            _logger.LogInformation($"List with id '{listId}' not found.");
            return CombinedResult<Guid, ListsError>.Failure(ListsError.ListNotFound);
        }
        else
        {
            if (!list.CanBeDeleted)
            {
                _logger.LogInformation($"List with id '{listId}' cannot be deleted due to hard lock.");
                return CombinedResult<Guid, ListsError>.Failure(ListsError.ListHardLocked);
            }
            else
            {
                var id = list.Id;
                _logger.LogInformation($"Deleting list '{listId}'");
                _yglDbContext.Lists.Remove(list);
                await _yglDbContext.SaveChangesAsync();
                return CombinedResult<Guid, ListsError>.Success(id);
            }
        }
    }

    #endregion

    #region ListEntry

    public async Task<CombinedResult<List<Guid>, ListsError>> AddListEntries(AddEntriesToListParameter parameters)
    {
        var list = await _yglDbContext.Lists
            .Include(x => x.Entries)
            .FirstOrDefaultAsync(x => x.UserId == parameters.UserInformation.UserId && x.Id == parameters.ListId);

        if (list == null)
        {
            _logger.LogInformation($"No lists found for id '{parameters.ListId}' and user '{parameters.UserInformation.UserId}'.");
            return CombinedResult<List<Guid>, ListsError>.Failure(ListsError.ListNotFound);
        }

        // Filter out entries that are already in the list
        var entriesToAdd = parameters.EntriesToAdd.Where(entryToAdd => list.Entries.All(x => x.GameId != entryToAdd.GameId)).ToList();

        if (entriesToAdd.Count == 0)
        {
            _logger.LogInformation("No new entries to add to the list.");
            return CombinedResult<List<Guid>, ListsError>.Success([]);
        }

        var listEntries = new List<GameListEntry>();
        foreach (var entryToAdd in entriesToAdd)
        {
            var listEntry = new GameListEntry()
            {
                GamesListId = list.Id,
                GameId = entryToAdd.GameId,
                Desc = entryToAdd.Desc ?? string.Empty,
                IsStarred = entryToAdd.IsStarred ?? false,
                Rating = entryToAdd.Rating ?? null,
                CompletionStatus = entryToAdd.CompletionStatus == null
                    ? CompletionStatus.Unspecified
                    : _yglDatabaseAndDtoMapper.Map(entryToAdd.CompletionStatus.Value),
                CreatedDate = _timeProvider.GetUtcNow()
            };

            if (entryToAdd.Platforms != null)
            {
                listEntry.Platforms = entryToAdd.Platforms.Select(x => _yglDatabaseAndDtoMapper.Map(x)).ToArray();
            }

            if (entryToAdd.GameDistributions != null)
            {
                listEntry.GameDistributions = entryToAdd.GameDistributions.Select(x => _yglDatabaseAndDtoMapper.Map(x)).ToArray();
            }

            listEntries.Add(listEntry);
        }

        list.LastModifiedDate = _timeProvider.GetUtcNow();
        await _yglDbContext.GameListEntries.AddRangeAsync(listEntries);
        await _yglDbContext.SaveChangesAsync();

        var listEntriesIds = listEntries.Select(x => x.Id).ToList();
        _logger.LogInformation($"Successfully added '{listEntriesIds.Count}' entries [{string.Join(',', listEntriesIds)}] to list '{list.Id}'.");
        return CombinedResult<List<Guid>, ListsError>.Success(listEntriesIds);
    }

    public async Task<CombinedResult<List<Guid>, ListsError>> DeleteListEntries(DeleteListEntriesParameter parameters)
    {
        var list = await _yglDbContext.Lists
            .Include(x => x.Entries)
            .FirstOrDefaultAsync(x => x.UserId == parameters.UserInformation.UserId && x.Id == parameters.ListId);

        if (list == null)
        {
            _logger.LogInformation($"No lists found for id '{parameters.ListId}' and user '{parameters.UserInformation.UserId}'.");
            return CombinedResult<List<Guid>, ListsError>.Failure(ListsError.ListNotFound);
        }

        var entriesToDelete = await _yglDbContext.GameListEntries
            .Where(e => ((IEnumerable<Guid>) parameters.EntriesToRemove).Contains(e.Id))
            .ToListAsync();

        if (entriesToDelete.Count == 0)
        {
            _logger.LogInformation($"No entries found to delete from the list {parameters.ListId}'.");
            return CombinedResult<List<Guid>, ListsError>.Success([]);
        }

        var listEntriesIds = entriesToDelete.Select(x => x.Id).ToList();

        _yglDbContext.GameListEntries.RemoveRange(entriesToDelete);
        await _yglDbContext.SaveChangesAsync();

        _logger.LogInformation($"Deleted '{listEntriesIds.Count}' entries [{string.Join(',', listEntriesIds)}] from list '{list.Id}'.");

        return CombinedResult<List<Guid>, ListsError>.Success(listEntriesIds);
    }

    public async Task<CombinedResult<List<Guid>, ListsError>> UpdateListEntries(UpdateListEntriesParameter parameters)
    {
        var list = await _yglDbContext.Lists
            .Include(x => x.Entries)
            .FirstOrDefaultAsync(x => x.UserId == parameters.UserInformation.UserId && x.Id == parameters.ListId);

        if (list == null)
        {
            _logger.LogInformation($"No lists found for id '{parameters.ListId}' and user '{parameters.UserInformation.UserId}'.");
            return CombinedResult<List<Guid>, ListsError>.Failure(ListsError.ListNotFound);
        }

        var updatedEntryIds = new List<Guid>();
        foreach (var entryToUpdateParameter in parameters.EntriesToUpdate)
        {
            var entry = list.Entries.FirstOrDefault(x => x.Id == entryToUpdateParameter.EntryId);
            if (entry == null)
            {
                _logger.LogInformation($"Entry to update '{entryToUpdateParameter.EntryId}' not found in the list '{list.Id}'.");
                continue;
            }

            entry.Desc = entryToUpdateParameter.Desc ?? entry.Desc;
            entry.Platforms = entryToUpdateParameter.Platforms != null
                ? entryToUpdateParameter.Platforms.Select(x => _yglDatabaseAndDtoMapper.Map(x)).ToArray()
                : entry.Platforms;
            entry.GameDistributions = entryToUpdateParameter.GameDistributions != null
                ? entryToUpdateParameter.GameDistributions.Select(x => _yglDatabaseAndDtoMapper.Map(x)).ToArray()
                : entry.GameDistributions;
            entry.IsStarred = entryToUpdateParameter.IsStarred ?? entry.IsStarred;
            entry.Rating = entryToUpdateParameter.Rating ?? entry.Rating;
            entry.CompletionStatus = entryToUpdateParameter.CompletionStatus != null
                ? _yglDatabaseAndDtoMapper.Map(entryToUpdateParameter.CompletionStatus.Value)
                : entry.CompletionStatus;

            entry.LastModifiedDate = _timeProvider.GetUtcNow();
            updatedEntryIds.Add(entry.Id);
        }

        list.LastModifiedDate = _timeProvider.GetUtcNow();
        await _yglDbContext.SaveChangesAsync();
        _logger.LogInformation($"Updated '{updatedEntryIds.Count}' entries [{string.Join(',', updatedEntryIds)}] from list '{list.Id}'.");
        return CombinedResult<List<Guid>, ListsError>.Success(updatedEntryIds);
    }

    #endregion

    private List<GamesListDto> GamesListToDto(IEnumerable<GamesList> gamesLists)
    {
        var listDtos = new List<GamesListDto>();
        foreach (var list in gamesLists)
        {
            var listDto = _yglDatabaseAndDtoMapper.Map(list);
            listDtos.Add(listDto);
        }

        return listDtos;
    }
}