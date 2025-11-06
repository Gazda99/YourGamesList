using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Model;
using YourGamesList.Api.Model.Dto;
using YourGamesList.Api.Services.ModelMapper;
using YourGamesList.Api.Services.Ygl.Lists.Model;
using YourGamesList.Common;
using YourGamesList.Database;
using YourGamesList.Database.Entities;

namespace YourGamesList.Api.Services.Ygl.Lists;

public interface IListsService
{
    #region List

    Task<CombinedResult<Guid, ListsError>> CreateList(JwtUserInformation userInfo, string listName, string description);
    Task<CombinedResult<List<GamesListDto>, ListsError>> SearchLists(SearchListsParameters parameters);
    Task<CombinedResult<List<GamesListDto>, ListsError>> GetSelfLists(JwtUserInformation userInfo, bool includeGames);
    Task<ErrorResult<ListsError>> UpdateLists(UpdateListsParameters parameters);
    Task<ErrorResult<ListsError>> DeleteList(JwtUserInformation userInfo, Guid listId);

    #endregion

    #region ListEntry

    Task<CombinedResult<List<Guid>, ListsError>> AddEntriesToList(AddEntriesToListParameter parameters);

    #endregion
}

//TODO: unit tests
public class ListsService : IListsService
{
    private readonly ILogger<ListsService> _logger;
    private readonly IYglDatabaseAndDtoMapper _yglDatabaseAndDtoMapper;
    private readonly YglDbContext _yglDbContext;

    #region List

    public ListsService(ILogger<ListsService> logger, IDbContextFactory<YglDbContext> yglDbContext, IYglDatabaseAndDtoMapper yglDatabaseAndDtoMapper)
    {
        _logger = logger;
        _yglDatabaseAndDtoMapper = yglDatabaseAndDtoMapper;
        _yglDbContext = yglDbContext.CreateDbContext();
    }

    public async Task<CombinedResult<Guid, ListsError>> CreateList(JwtUserInformation userInfo, string listName, string description)
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
            Desc = description,
            IsPublic = true,
            CanBeDeleted = true
        };

        await _yglDbContext.Lists.AddAsync(list);
        await _yglDbContext.SaveChangesAsync();
        _logger.LogInformation($"Created new list with id '{list.Id}' for user '{userInfo.UserId}'.");
        return CombinedResult<Guid, ListsError>.Success(list.Id);
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
            listsQuery = listsQuery.Include(x => x.Games);
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
        var listsQuery = _yglDbContext.Lists
            .AsNoTracking();

        if (includeGames)
        {
            listsQuery = listsQuery.Include(x => x.Games).ThenInclude(x => x.Game);
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

    public async Task<ErrorResult<ListsError>> UpdateLists(UpdateListsParameters parameters)
    {
        var list = await _yglDbContext.Lists.FirstOrDefaultAsync(x => x.UserId == parameters.UserInformation.UserId && x.Id == parameters.ListId);
        if (list == null)
        {
            _logger.LogInformation($"No lists found for id '{parameters.ListId}' and user '{parameters.UserInformation.UserId}'.");
            return ErrorResult<ListsError>.Failure(ListsError.ListNotFound);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Name) && !list.Name.Equals(parameters.Name, StringComparison.CurrentCultureIgnoreCase))
        {
            var nameExists =
                await _yglDbContext.Lists.AnyAsync(x => x.UserId == list.UserId && x.Id != list.Id && x.Name.ToLower() == parameters.Name.ToLower());
            if (nameExists)
            {
                _logger.LogInformation($"Cannot rename list to '{parameters.Name}' because it already exists for user '{list.UserId}'.");
                return ErrorResult<ListsError>.Failure(ListsError.ListAlreadyExists);
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

        await _yglDbContext.SaveChangesAsync();

        _logger.LogInformation("List updated successfully.");
        return ErrorResult<ListsError>.Clear();
    }

    public async Task<ErrorResult<ListsError>> DeleteList(JwtUserInformation userInfo, Guid listId)
    {
        _logger.LogInformation($"Search for list '{listId}' which belongs to user '{userInfo.Username}'.");
        var list = await _yglDbContext.Lists.FirstOrDefaultAsync(x =>
            x.UserId == userInfo.UserId &&
            x.Id == listId
        );

        if (list == null)
        {
            _logger.LogInformation($"List with id '{listId}' not found.");
            return ErrorResult<ListsError>.Failure(ListsError.ListNotFound);
        }
        else
        {
            if (!list.CanBeDeleted)
            {
                _logger.LogInformation($"List with id '{listId}' cannot be deleted due to hard lock.");
                return ErrorResult<ListsError>.Failure(ListsError.ListHardLocked);
            }
            else
            {
                _logger.LogInformation($"Deleting list '{listId}'");
                _yglDbContext.Lists.Remove(list);
                await _yglDbContext.SaveChangesAsync();
                return ErrorResult<ListsError>.Clear();
            }
        }
    }

    #endregion

    #region ListEntry

    public async Task<CombinedResult<List<Guid>, ListsError>> AddEntriesToList(AddEntriesToListParameter parameters)
    {
        var list = await _yglDbContext.Lists
            .Include(x => x.Games)
            .FirstOrDefaultAsync(x => x.UserId == parameters.UserInformation.UserId && x.Id == parameters.ListId);

        if (list == null)
        {
            _logger.LogInformation($"No lists found for id '{parameters.ListId}' and user '{parameters.UserInformation.UserId}'.");
            return CombinedResult<List<Guid>, ListsError>.Failure(ListsError.ListNotFound);
        }

        // Filter out entries that are already in the list
        var entriesToAdd = parameters.EntriesToAdd.Where(entryToAdd => list.Games.All(x => x.GameId != entryToAdd.GameId)).ToList();

        if (entriesToAdd.Count == 0)
        {
            
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
                    : _yglDatabaseAndDtoMapper.Map(entryToAdd.CompletionStatus.Value)
            };

            if (entryToAdd.Platforms != null)
            {
                listEntry.Platforms = entryToAdd.Platforms.Select(x => _yglDatabaseAndDtoMapper.Map(x)).ToArray();
            }

            if (entryToAdd.GameDistributions != null)
            {
                listEntry.GameDistributions = entryToAdd.GameDistributions.Select(x => _yglDatabaseAndDtoMapper.Map(x)).ToArray();
            }

            // await _yglDbContext.GameListEntries.AddAsync(listEntry);
            listEntries.Add(listEntry);
        }

        await _yglDbContext.GameListEntries.AddRangeAsync(listEntries);
        await _yglDbContext.SaveChangesAsync();

        var gameIds = entriesToAdd.Select(x => x.GameId).ToList();
        return CombinedResult<List<Guid>, ListsError>.Success(gameIds);
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