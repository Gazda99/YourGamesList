using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Services.ModelMappers;
using YourGamesList.Api.Services.Users.Model;
using YourGamesList.Common;
using YourGamesList.Contracts.Dto;
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
    private readonly ICountriesService _countriesService;
    private readonly TimeProvider _timeProvider;
    private readonly YglDbContext _yglDbContext;

    public UsersService(ILogger<UsersService> logger, IDbContextFactory<YglDbContext> yglDbContext, IYglDatabaseAndDtoMapper yglDatabaseAndDtoMapper,
        ICountriesService countriesService,
        TimeProvider timeProvider)
    {
        _logger = logger;
        _yglDatabaseAndDtoMapper = yglDatabaseAndDtoMapper;
        _countriesService = countriesService;
        _timeProvider = timeProvider;
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
        if (!ValidateUserUpdateParameters(parameters))
        {
            _logger.LogWarning($"User with ID '{parameters.UserInformation.UserId}' cannot be updated due to validation errors.");
            return CombinedResult<Guid, UsersError>.Failure(UsersError.UserUpdateWrongInputData);
        }

        var user = await _yglDbContext.Users.FirstOrDefaultAsync(x => x.Id == parameters.UserInformation.UserId);
        if (user == null)
        {
            _logger.LogWarning($"User with ID '{parameters.UserInformation.UserId}' not found.");
            return CombinedResult<Guid, UsersError>.Failure(UsersError.UserNotFound);
        }

        user.Country = parameters.Country ?? string.Empty;
        user.Description = parameters.Description ?? string.Empty;
        user.DateOfBirth = parameters.DateOfBirth;
        user.LastModifiedDate = _timeProvider.GetUtcNow();

        await _yglDbContext.SaveChangesAsync();
        _logger.LogInformation($"Updated user with ID '{user.Id}'");

        return CombinedResult<Guid, UsersError>.Success(user.Id);
    }

    private void LogValidationError(string reason)
    {
        _logger.LogInformation($"Validation of '{nameof(UserUpdateParameters)}' failed. Reason: {reason}.");
    }

    private bool ValidateUserUpdateParameters(UserUpdateParameters parameters)
    {
        if (!_countriesService.ValidateThreeLetterIsoCode(parameters.Country))
        {
            LogValidationError("wrong country name");
            return false;
        }

        if (parameters.DateOfBirth != null)
        {
            var now = _timeProvider.GetUtcNow().DateTime;
            var dob = parameters.DateOfBirth.Value;

            var age = now.Year - dob.Year;

            // If the birthday hasn't occurred yet this year, subtract one year
            if (dob.Date > now.AddYears(-age))
            {
                age--;
            }

            if (age < 12)
            {
                LogValidationError("wrong date of birth");
                return false;
            }
        }

        if (parameters.Description?.Length > 512)
        {
            LogValidationError("wrong description");
            return false;
        }

        return true;
    }
}