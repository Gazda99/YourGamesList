using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Model;
using YourGamesList.Api.Services.Lists.Model;
using YourGamesList.Common;
using YourGamesList.Contracts.Dto;
using YourGamesList.Database;
using YourGamesList.Database.Entity;

namespace YourGamesList.Api.Services.Lists;

public interface IListsManagerService
{
    Task<CombinedResult<List<GamesListDto>, ListsError>> GetSelf(GetSelfListParams getSelfListParams);
    Task<CombinedResult<Guid, ListsError>> Create(CreateListParams createListParams);
    Task<ErrorResult<ListsError>> Delete(DeleteListParams deleteListParams);
}

public class ListsManagerService : IListsManagerService
{
    private readonly ILogger<ListsManagerService> _logger;
    private readonly YourGamesListDbContext _yglDbContext;
    private readonly TimeProvider _timeProvider;

    public ListsManagerService(
        ILogger<ListsManagerService> logger,
        IDbContextFactory<YourGamesListDbContext> yglDbContext,
        TimeProvider timeProvider)
    {
        _logger = logger;
        _yglDbContext = yglDbContext.CreateDbContext();
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
            IsPrivate = list.IsPrivate,
            Name = list.Name,
            GameListEntries = list.GamesListEntries.Select(entry => new GamesListEntryDto
            {
                CompletionStatus = (CompletionStatusDto)entry.CompletionStatus,
                Description = entry.Description,
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
        if (createListParams.Name.Length > ValidationConstants.GamesList.MaximumNameLength)
        {
            return CombinedResult<Guid, ListsError>.Failure(ListsError.ListNameTooLong);
        }

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
            CanBeDeleted = true,
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

        if (!gamesList.CanBeDeleted)
        {
            _logger.LogInformation("Cannot delete protected list {GamesListId}.", deleteListParams.Id);
            return ErrorResult<ListsError>.Failure(ListsError.ListCannotBeDeleted);
        }

        _yglDbContext.GamesLists.Remove(gamesList);
        await _yglDbContext.SaveChangesAsync();

        return ErrorResult<ListsError>.Success();
    }

}