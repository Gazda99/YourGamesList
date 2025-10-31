using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Services.Auth.Model;
using YourGamesList.Common;
using YourGamesList.Database;
using YourGamesList.Database.Entities;

namespace YourGamesList.Api.Services.Auth;

public interface IUserManagerService
{
    Task<ErrorResult<UserAuthError>> RegisterUser(string username, string password);
    Task<CombinedResult<string, UserAuthError>> Login(string username, string password);
    Task<ErrorResult<UserAuthError>> Delete(string username, string password);
}

//TODO: unit tests
public class UserManagerService : IUserManagerService
{
    private readonly ILogger<UserManagerService> _logger;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IPasswordValidator _passwordValidator;
    private readonly ITokenProvider _tokenProvider;
    private readonly TimeProvider _timeProvider;
    private readonly YglDbContext _yglDbContext;

    public UserManagerService(
        ILogger<UserManagerService> logger,
        IPasswordHasher passwordHasher,
        IPasswordValidator passwordValidator,
        ITokenProvider tokenProvider,
        IDbContextFactory<YglDbContext> yglDbContext,
        TimeProvider timeProvider
    )
    {
        _logger = logger;
        _passwordHasher = passwordHasher;
        _passwordValidator = passwordValidator;
        _tokenProvider = tokenProvider;
        _timeProvider = timeProvider;
        _yglDbContext = yglDbContext.CreateDbContext();
    }

    public async Task<ErrorResult<UserAuthError>> RegisterUser(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            _logger.LogWarning("Username is null or empty.");
            return ErrorResult<UserAuthError>.Failure(UserAuthError.InvalidUsername);
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning("Password is null or empty.");
            return ErrorResult<UserAuthError>.Failure(UserAuthError.WrongPassword);
        }

        username = username.ToLower();

        var doesUserExists = await FindUserByUserName(username);
        if (doesUserExists.IsSuccess)
        {
            _logger.LogInformation($"User with username '{username}' already exists.");
            return ErrorResult<UserAuthError>.Failure(UserAuthError.RegisterNameAlreadyTaken);
        }

        var isPasswordValid = _passwordValidator.ValidatePassword(password);
        if (isPasswordValid.IsFailure)
        {
            _logger.LogWarning($"Password for the username '{username}' is invalid. Reason: '{isPasswordValid.Error.ToString()}'");
            return ErrorResult<UserAuthError>.Failure(isPasswordValid.Error);
        }

        var hashedPassword = _passwordHasher.HashPassword(password);

        var now = _timeProvider.GetUtcNow();

        var newUser = new User()
        {
            Username = username,
            PasswordHash = hashedPassword.HashString,
            Salt = hashedPassword.Salt,
            CreatedDate = now
        };

        _yglDbContext.Users.Add(newUser);
        await _yglDbContext.SaveChangesAsync();

        _logger.LogInformation($"User with username '{username}' registered successfully.");

        return ErrorResult<UserAuthError>.Clear();
    }

    public async Task<CombinedResult<string, UserAuthError>> Login(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            _logger.LogWarning("Username is null or empty.");
            return CombinedResult<string, UserAuthError>.Failure(UserAuthError.InvalidUsername);
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning("Password is null or empty.");
            return CombinedResult<string, UserAuthError>.Failure(UserAuthError.WrongPassword);
        }

        username = username.ToLower();

        var findUser = await FindUserByUserName(username);
        if (findUser.IsFailure)
        {
            _logger.LogWarning($"User with the username '{username}' was not found.");
            return CombinedResult<string, UserAuthError>.Failure(UserAuthError.NoUserFound);
        }

        var user = findUser.Value;

        if (!string.Equals(user.PasswordHash, _passwordHasher.HashPassword(password, user.Salt).HashString))
        {
            _logger.LogWarning($"Provided password for '{username}' was invalid.");
            return CombinedResult<string, UserAuthError>.Failure(UserAuthError.WrongPassword);
        }

        var token = _tokenProvider.CreateToken(user.Username, user.Id);
        _logger.LogInformation($"User with username '{username}' logged in successfully.");

        return CombinedResult<string, UserAuthError>.Success(token);
    }

    public async Task<ErrorResult<UserAuthError>> Delete(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            _logger.LogWarning("Username is null or empty.");
            return ErrorResult<UserAuthError>.Failure(UserAuthError.InvalidUsername);
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning("Password is null or empty.");
            return ErrorResult<UserAuthError>.Failure(UserAuthError.WrongPassword);
        }

        var findUser = await FindUserByUserName(username);
        if (findUser.IsFailure)
        {
            _logger.LogWarning($"User with the username '{username}' was not found.");
            return ErrorResult<UserAuthError>.Failure(UserAuthError.NoUserFound);
        }

        var user = findUser.Value;
        // Check if the provided password is correct
        if (!string.Equals(user.PasswordHash, _passwordHasher.HashPassword(password, user.Salt).HashString))
        {
            _logger.LogWarning($"Provided password for '{username}' was invalid.");
            return ErrorResult<UserAuthError>.Failure(UserAuthError.WrongPassword);
        }

        _yglDbContext.Users.Remove(user);
        await _yglDbContext.SaveChangesAsync();

        _logger.LogInformation($"User with username '{username}' removed successfully.");

        return ErrorResult<UserAuthError>.Clear();
    }


    private async Task<ValueResult<User>> FindUserByUserName(string username)
    {
        var user = await _yglDbContext.Users.FirstOrDefaultAsync(x => string.Equals(x.Username, username));
        return user == null ? ValueResult<User>.Failure() : ValueResult<User>.Success(user);
    }
}