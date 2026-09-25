using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YourGamesList.Api.Services.Auth.Model;
using YourGamesList.Api.Services.Auth.Options;
using YourGamesList.Common;
using YourGamesList.Database;
using YourGamesList.Database.Entity;

namespace YourGamesList.Api.Services.Auth;

//TODO: Unit tests

public interface IUserManagerService
{
    Task<CombinedResult<Guid, UserAuthError>> Register(UserRegisterDto userRegisterDto);
    Task Login(UserLoginDto userLoginDto);
    Task Delete(UserDeleteDto userDeleteDto);
}

public class UserManagerService : IUserManagerService
{
    private readonly ILogger<UserManagerService> _logger;
    private readonly IOptions<UserManagerServiceOptions> _options;
    private readonly IUserValuesValidator _userValuesValidator;
    private readonly IPasswordHasher _passwordHasher;
    private readonly TimeProvider _timeProvider;
    private readonly IVerificationTokenHasher _verificationTokenHasher;
    private readonly IVerificationEmailSender _verificationEmailSender;
    private readonly YourGamesListDbContext _yglDbContext;

    public UserManagerService(ILogger<UserManagerService> logger,
        IOptions<UserManagerServiceOptions> options,
        IUserValuesValidator userValuesValidator,
        IDbContextFactory<YourGamesListDbContext> yglDbContext,
        IPasswordHasher passwordHasher,
        TimeProvider timeProvider,
        IVerificationTokenHasher verificationTokenHasher,
        IVerificationEmailSender verificationEmailSender
    )
    {
        _logger = logger;
        _options = options;
        _userValuesValidator = userValuesValidator;
        _passwordHasher = passwordHasher;
        _timeProvider = timeProvider;
        _verificationTokenHasher = verificationTokenHasher;
        _verificationEmailSender = verificationEmailSender;
        _yglDbContext = yglDbContext.CreateDbContext();
    }

    public async Task Login(UserLoginDto userLoginDto)
    {
        throw new System.NotImplementedException();
    }

    public async Task<CombinedResult<Guid, UserAuthError>> Register(UserRegisterDto userRegisterDto)
    {
        var username = userRegisterDto.Username.ToLowerInvariant().Trim();
        var email = userRegisterDto.Email.ToLowerInvariant().Trim();
        var dob = userRegisterDto.DateOfBirth;

        var validateEmail = _userValuesValidator.ValidateEmail(email);
        if (validateEmail.IsFailure)
        {
            _logger.LogInformation("Cannot register user due to email being invalid. Error: {UserRegisterError}", validateEmail.Error.ToString());
            return CombinedResult<Guid, UserAuthError>.Failure(validateEmail.Error);
        }

        var validateUsername = _userValuesValidator.ValidateUsername(username);
        if (validateUsername.IsFailure)
        {
            _logger.LogInformation("Cannot register user due to username being invalid. Error: {UserRegisterError}", validateUsername.Error.ToString());
            return CombinedResult<Guid, UserAuthError>.Failure(validateUsername.Error);
        }

        var validatePasswordResult = _userValuesValidator.ValidatePassword(userRegisterDto.Password);
        if (validatePasswordResult.IsFailure)
        {
            _logger.LogInformation("Cannot register user due to password being invalid. Error: {UserRegisterError}", validatePasswordResult.Error.ToString());
            return CombinedResult<Guid, UserAuthError>.Failure(validatePasswordResult.Error);
        }

        var validateDateOfBirth = _userValuesValidator.ValidateDateOfBirth(dob);
        if (validateDateOfBirth.IsFailure)
        {
            _logger.LogInformation("Cannot register user due to date of birth being invalid. Error: {UserRegisterError}", validateDateOfBirth.Error.ToString());
            return CombinedResult<Guid, UserAuthError>.Failure(validateDateOfBirth.Error);
        }

        var doesUsernameExists = _yglDbContext.Users.AnyAsync(u => u.Username == username);
        var doesEmailExists = _yglDbContext.Users.AnyAsync(u => u.Email == email);
        var dbChecks = Task.WhenAll(doesUsernameExists, doesEmailExists);
        try
        {
            await dbChecks;
        }
        catch (Exception)
        {
            _logger.LogError(dbChecks.Exception, "Register failed while checking username and email availability in db.");
            return CombinedResult<Guid, UserAuthError>.Failure(UserAuthError.InternalServerError);
        }

        if (doesEmailExists.Result)
        {
            return CombinedResult<Guid, UserAuthError>.Failure(UserAuthError.EmailAlreadyTaken);
        }

        if (doesUsernameExists.Result)
        {
            return CombinedResult<Guid, UserAuthError>.Failure(UserAuthError.UsernameAlreadyTaken);
        }

        var hashedPassword = _passwordHasher.HashPassword(userRegisterDto.Password);
        if (hashedPassword.IsFailure)
        {
            _logger.LogError("Hashing of password failed.");
            return CombinedResult<Guid, UserAuthError>.Failure(UserAuthError.InternalServerError);
        }

        var now = _timeProvider.GetUtcNow();

        if (!_options.Value.EnableEmailConfirmation)
        {
            _logger.LogInformation("Email confirmation is disabled. Proceeding with adding the User with status {UserStatus}", nameof(UserStatus.Active));

            var user = new User()
            {
                CreatedAt = now,
                DateOfBirth = dob,
                Email = email,
                Username = username,
                PasswordHash = hashedPassword.Value,
                UserStatus = UserStatus.Active,
                ExpiryAt = null
            };
            _yglDbContext.Users.Add(user);
            await _yglDbContext.SaveChangesAsync();
            _logger.LogInformation("Added user to database");
        }
        else
        {
            _logger.LogInformation("Email confirmation is enabled. Proceeding with adding the User with status {UserStatus}", nameof(UserStatus.Pending));

            var userExpiration = now + TimeSpan.FromMinutes(_options.Value.DefaultPendingUserExpiryInMinutes);
            var user = new User()
            {
                CreatedAt = now,
                DateOfBirth = dob,
                Email = email,
                Username = username,
                PasswordHash = hashedPassword.Value,
                UserStatus = UserStatus.Pending,
                ExpiryAt = userExpiration
            };

            _yglDbContext.Users.Add(user);

            var emailVerificationTokens = _verificationTokenHasher.GenerateToken();
            now = _timeProvider.GetUtcNow();

            var wasEmailSent = await _verificationEmailSender.SendVerificationEmail(email, username, emailVerificationTokens.RawToken);
            if (!wasEmailSent)
            {
                _logger.LogWarning("Email verification email was not sent!");
                _yglDbContext.Users.Remove(user);
                return CombinedResult<Guid, UserAuthError>.Failure(UserAuthError.InternalServerError);
            }

            var emailVerificationExpiration = now + TimeSpan.FromMinutes(_options.Value.DefaultEmailVerificationExpiryInMinutes);
            var emailVerification = new EmailVerification()
            {
                CreatedAt = now,
                ExpiryAt = emailVerificationExpiration,
                Token = emailVerificationTokens.TokenHash,
                User = user
            };

            _yglDbContext.EmailVerifications.Add(emailVerification);
            await _yglDbContext.SaveChangesAsync();
            _logger.LogInformation("Added user to database");
        }

        return CombinedResult<Guid, UserAuthError>.Success(Guid.NewGuid());
    }

    public async Task Delete(UserDeleteDto userDeleteDto)
    {
        throw new System.NotImplementedException();
    }
}