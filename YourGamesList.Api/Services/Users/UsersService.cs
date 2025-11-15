using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Model.Dto;
using YourGamesList.Api.Services.ModelMappers;
using YourGamesList.Api.Services.Users.Model;
using YourGamesList.Common;
using YourGamesList.Database;

namespace YourGamesList.Api.Services.Users;

public interface IUsersService
{
    Task<CombinedResult<UserDto, UsersError>> GetSelfUser(UserGetSelfParameters parameters);
    Task<CombinedResult<Guid, UsersError>> UpdateUser(UserUpdateParameters parameters);
}

public class UsersService : IUsersService
{
    private readonly ILogger<UsersService> _logger;
    private readonly IYglDatabaseAndDtoMapper _yglDatabaseAndDtoMapper;
    private readonly YglDbContext _yglDbContext;

    public UsersService(ILogger<UsersService> logger, IDbContextFactory<YglDbContext> yglDbContext, IYglDatabaseAndDtoMapper yglDatabaseAndDtoMapper)
    {
        _logger = logger;
        _yglDatabaseAndDtoMapper = yglDatabaseAndDtoMapper;
        _yglDbContext = yglDbContext.CreateDbContext();
    }

    public async Task<CombinedResult<UserDto, UsersError>> GetSelfUser(UserGetSelfParameters parameters)
    {
        var user = await _yglDbContext.Users.FirstOrDefaultAsync(x => x.Id == parameters.UserInformation.UserId);
        if (user == null)
        {
            _logger.LogWarning($"User with ID '{parameters.UserInformation.UserId}' not found.");
            return CombinedResult<UserDto, UsersError>.Failure(UsersError.UserNotFound);
        }

        var userDto = _yglDatabaseAndDtoMapper.Map(user);
        _logger.LogInformation($"Retrieved user with ID '{user.Id}'");
        return CombinedResult<UserDto, UsersError>.Success(userDto);
    }

    public async Task<CombinedResult<Guid, UsersError>> UpdateUser(UserUpdateParameters parameters)
    {
        var user = await _yglDbContext.Users.FirstOrDefaultAsync(x => x.Id == parameters.UserInformation.UserId);
        if (user == null)
        {
            _logger.LogWarning($"User with ID '{parameters.UserInformation.UserId}' not found.");
            return CombinedResult<Guid, UsersError>.Failure(UsersError.UserNotFound);
        }

        user.Country = parameters.Country ?? string.Empty;
        user.Description = parameters.Description ?? string.Empty;
        user.DateOfBirth = parameters.DateOfBirth;

        await _yglDbContext.SaveChangesAsync();
        _logger.LogInformation($"Updated user with ID '{user.Id}'");

        return CombinedResult<Guid, UsersError>.Success(user.Id);
    }
}