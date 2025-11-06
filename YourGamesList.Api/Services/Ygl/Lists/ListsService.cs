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
    Task<CombinedResult<Guid, ListsError>> CreateList(JwtUserInformation userInfo, string listName, string description);
    Task<CombinedResult<List<GamesListDto>, ListsError>> SearchLists(SearchListsParameters parameters);
    Task<CombinedResult<List<GamesListDto>, ListsError>> GetSelfLists(JwtUserInformation userInfo, bool includeGames);
    Task<ErrorResult<ListsError>> UpdateLists(UpdateListsParameters parameters);
    Task<ErrorResult<ListsError>> DeleteList(JwtUserInformation userInfo, string listName);
}

//TODO: unit tests
public class ListsService : IListsService
{
    private readonly ILogger<ListsService> _logger;
    private readonly IYglDatabaseToDtoMapper _yglDatabaseToDtoMapper;
    private readonly YglDbContext _yglDbContext;

    public ListsService(ILogger<ListsService> logger, IDbContextFactory<YglDbContext> yglDbContext, IYglDatabaseToDtoMapper yglDatabaseToDtoMapper)
    {
        _logger = logger;
        _yglDatabaseToDtoMapper = yglDatabaseToDtoMapper;
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
        var listsQuery = _yglDbContext.Lists.AsNoTracking().Where(x => x.UserId == userInfo.UserId);
        if (includeGames)
        {
            listsQuery = listsQuery.Include(x => x.Games);
        }

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
        var list = await _yglDbContext.Lists.FirstOrDefaultAsync(x => x.Id == parameters.Id);
        if (list == null)
        {
            _logger.LogInformation($"No lists found for id '{parameters.Id}'.");
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

    public async Task<ErrorResult<ListsError>> DeleteList(JwtUserInformation userInfo, string listName)
    {
        _logger.LogInformation($"Search for list '{listName}' which belongs to user '{userInfo.Username}'.");
        var list = await _yglDbContext.Lists.FirstOrDefaultAsync(x =>
            x.UserId == userInfo.UserId &&
            x.Name.ToLower() == listName.ToLower()
        );

        if (list == null)
        {
            _logger.LogInformation($"List with name '{listName}' not found.");
            return ErrorResult<ListsError>.Failure(ListsError.ListNotFound);
        }
        else
        {
            if (!list.CanBeDeleted)
            {
                _logger.LogInformation($"List with name '{listName}' cannot be deleted due to hard lock.");
                return ErrorResult<ListsError>.Failure(ListsError.ListHardLocked);
            }
            else
            {
                _logger.LogInformation($"Deleting list '{listName}'");
                _yglDbContext.Lists.Remove(list);
                await _yglDbContext.SaveChangesAsync();
                return ErrorResult<ListsError>.Clear();
            }
        }
    }

    private List<GamesListDto> GamesListToDto(IEnumerable<GamesList> gamesLists)
    {
        var listDtos = new List<GamesListDto>();
        foreach (var list in gamesLists)
        {
            var listDto = _yglDatabaseToDtoMapper.Map(list);
            listDtos.Add(listDto);
        }

        return listDtos;
    }
}