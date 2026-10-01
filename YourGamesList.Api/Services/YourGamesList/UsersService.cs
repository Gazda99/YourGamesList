using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Services.YourGamesList.Model;
using YourGamesList.Common;
using YourGamesList.Contracts.Dto;
using YourGamesList.Database;
using YourGamesList.Database.Entity;

namespace YourGamesList.Api.Services.YourGamesList;

public interface IUsersService
{
    public Task<CombinedResult<UserDto, UsersError>> GetSelfUser(UserGetSelfParams userGetSelfParams);
}

public class UsersService : IUsersService
{
    public readonly ILogger<UsersService> _logger;
    private readonly YourGamesListDbContext _yglDbContext;

    public UsersService(
        ILogger<UsersService> logger,
        IDbContextFactory<YourGamesListDbContext> yglDbContext
    )
    {
        _logger = logger;
        _yglDbContext = yglDbContext.CreateDbContext();
    }

    public async Task<CombinedResult<UserDto, UsersError>> GetSelfUser(UserGetSelfParams userGetSelfParams)
    {
        var userId = userGetSelfParams.UserInformation.UserId;

        var getUserRes = await FindUser(userId);
        if (getUserRes.IsFailure)
        {
            _logger.LogInformation("Cannot find user.");
            return CombinedResult<UserDto, UsersError>.Failure(getUserRes.Error);
        }

        var user = getUserRes.Value;
        var gamesLists = user.GamesLists.Select(x => new GamesListDto()
        {
            CreatedAt = x.CreatedAt,
            Description = x.Description,
            IsPrivate = x.IsPrivate,
            Name = x.Name,
        }).ToList();

        var userDto = new UserDto()
        {
            CreatedAt = user.CreatedAt,
            Id = user.Id,
            Username = user.Username,
            Description = user.Description,
            DateOfBirth = user.DateOfBirth,
            GamesLists = gamesLists
        };
        return CombinedResult<UserDto, UsersError>.Success(userDto);
    }

    private async Task<CombinedResult<User, UsersError>> FindUser(Guid userId)
    {
        var user = await _yglDbContext.Users
            .AsNoTracking()
            .AsSplitQuery()
            .Include(user => user.GamesLists)
            .FirstOrDefaultAsync(x => x.Id == userId);

        if (user == null)
        {
            return CombinedResult<User, UsersError>.Failure(UsersError.UserNotFound);
        }

        return CombinedResult<User, UsersError>.Success(user);
    }
}