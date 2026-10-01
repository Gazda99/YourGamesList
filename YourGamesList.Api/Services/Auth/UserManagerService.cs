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

public interface IUserManagerService
{
    Task<CombinedResult<Guid, UserAuthError>> Register(UserRegisterDto userRegisterDto);
    Task<CombinedResult<string, UserAuthError>> Login(UserLoginDto userLoginDto);
    Task<ErrorResult<UserAuthError>> Delete(UserDeleteDto userDeleteDto);
    Task<ErrorResult<UserAuthError>> ConfirmEmail(ConfirmEmailDto confirmEmailDto);
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
    private readonly ITokenProvider _tokenProvider;
    private readonly YourGamesListDbContext _yglDbContext;

    public UserManagerService(ILogger<UserManagerService> logger,
        IOptions<UserManagerServiceOptions> options,
        IUserValuesValidator userValuesValidator,
        IDbContextFactory<YourGamesListDbContext> yglDbContext,
        IPasswordHasher passwordHasher,
        TimeProvider timeProvider,
        IVerificationTokenHasher verificationTokenHasher,
        IVerificationEmailSender verificationEmailSender,
        ITokenProvider tokenProvider
    )
    {
        _logger = logger;
        _options = options;
        _userValuesValidator = userValuesValidator;
        _passwordHasher = passwordHasher;
        _timeProvider = timeProvider;
        _verificationTokenHasher = verificationTokenHasher;
        _verificationEmailSender = verificationEmailSender;
        _tokenProvider = tokenProvider;
        _yglDbContext = yglDbContext.CreateDbContext();
    }

    public async Task<CombinedResult<string, UserAuthError>> Login(UserLoginDto userLoginDto)
    {
        var username = userLoginDto.Username.ToLowerInvariant().Trim();
        var user = await _yglDbContext.Users.FirstOrDefaultAsync(u => u.Username == username);

        if (user == null)
        {
            _logger.LogInformation("Not user found.");
            return CombinedResult<string, UserAuthError>.Failure(UserAuthError.NoUserFound);
        }

        if (user.UserStatus == UserStatus.Pending)
        {
            _logger.LogInformation("User status is {UserStatus}.", nameof(UserStatus.Pending));
            return CombinedResult<string, UserAuthError>.Failure(UserAuthError.UserNotActiveEmailActivation);
        }

        var hashedPassword = user.PasswordHash;
        var passwordMatch = _passwordHasher.VerifyHashedPassword(userLoginDto.Password, hashedPassword);

        if (!passwordMatch)
        {
            _logger.LogInformation("Password does not match.");
            return CombinedResult<string, UserAuthError>.Failure(UserAuthError.UnauthorizedUser);
        }

        var newToken = _tokenProvider.CreateToken(user.Username, user.Id);
        _logger.LogInformation("Created new user token. User logged in.");

        var now = _timeProvider.GetUtcNow();
        user.LastLoginDate = now;
        await _yglDbContext.SaveChangesAsync();

        return CombinedResult<string, UserAuthError>.Success(newToken);
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
            _logger.LogInformation("Email already registered.");
            return CombinedResult<Guid, UserAuthError>.Failure(UserAuthError.EmailAlreadyTaken);
        }

        if (doesUsernameExists.Result)
        {
            _logger.LogInformation("Username already registered.");
            return CombinedResult<Guid, UserAuthError>.Failure(UserAuthError.UsernameAlreadyTaken);
        }

        var hashedPassword = _passwordHasher.HashPassword(userRegisterDto.Password);
        if (hashedPassword.IsFailure)
        {
            _logger.LogError("Hashing of password failed.");
            return CombinedResult<Guid, UserAuthError>.Failure(UserAuthError.InternalServerError);
        }

        var now = _timeProvider.GetUtcNow();


        User user;
        if (!_options.Value.EnableEmailConfirmation)
        {
            _logger.LogInformation("Email confirmation is disabled. Proceeding with adding the User with status {UserStatus}", nameof(UserStatus.Active));

            user = new User()
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
        }
        else
        {
            _logger.LogInformation("Email confirmation is enabled. Proceeding with adding the User with status {UserStatus}", nameof(UserStatus.Pending));

            var userExpiration = now + TimeSpan.FromMinutes(_options.Value.DefaultPendingUserExpiryInMinutes);
            user = new User()
            {
                CreatedAt = now,
                DateOfBirth = dob,
                Email = email,
                Username = username,
                PasswordHash = hashedPassword.Value,
                UserStatus = UserStatus.Pending,
                ExpiryAt = userExpiration
            };

            var emailVerificationTokens = _verificationTokenHasher.GenerateToken();
            now = _timeProvider.GetUtcNow();

            var wasEmailSent = await _verificationEmailSender.SendVerificationEmail(email, username, emailVerificationTokens.RawToken);
            if (!wasEmailSent)
            {
                _logger.LogWarning("Email verification was not sent!");
                return CombinedResult<Guid, UserAuthError>.Failure(UserAuthError.InternalServerError);
            }
            _logger.LogInformation("Email verification sent!");

            _yglDbContext.Users.Add(user);
            
            var emailVerificationExpiration = now + TimeSpan.FromMinutes(_options.Value.DefaultEmailVerificationExpiryInMinutes);
            var emailVerification = new EmailVerification()
            {
                CreatedAt = now,
                ExpiryAt = emailVerificationExpiration,
                Token = emailVerificationTokens.TokenHash,
                User = user
            };

            _yglDbContext.EmailVerifications.Add(emailVerification);
        }

        await _yglDbContext.SaveChangesAsync();
        _logger.LogInformation("Added user to database");

        return CombinedResult<Guid, UserAuthError>.Success(user.Id);
    }

    public async Task<ErrorResult<UserAuthError>> Delete(UserDeleteDto userDeleteDto)
    {
        var username = userDeleteDto.Username.ToLowerInvariant().Trim();
        var user = await _yglDbContext.Users.FirstOrDefaultAsync(u => u.Username == username);

        if (user == null)
        {
            _logger.LogInformation("Not user found.");
            return ErrorResult<UserAuthError>.Failure(UserAuthError.NoUserFound);
        }

        var hashedPassword = user.PasswordHash;
        var passwordMatch = _passwordHasher.VerifyHashedPassword(userDeleteDto.Password, hashedPassword);

        if (!passwordMatch)
        {
            _logger.LogInformation("Password does not match.");
            return ErrorResult<UserAuthError>.Failure(UserAuthError.UnauthorizedUser);
        }

        _yglDbContext.Users.Remove(user);
        await _yglDbContext.SaveChangesAsync();
        _logger.LogInformation("Removed user from database.");

        return ErrorResult<UserAuthError>.Success();
    }

    public async Task<ErrorResult<UserAuthError>> ConfirmEmail(ConfirmEmailDto confirmEmailDto)
    {
        var token = confirmEmailDto.Token.ToLower();
        if (string.IsNullOrWhiteSpace(token))
        {
            return ErrorResult<UserAuthError>.Failure(UserAuthError.EmailVerificationInvalid);
        }
        
        var hashToken = _verificationTokenHasher.HashToken(token);
        var emailVerification = await _yglDbContext.EmailVerifications.FirstOrDefaultAsync(x => x.Token == hashToken);
        if (emailVerification == null)
        {
            _logger.LogInformation("Email verification not found.");
            return ErrorResult<UserAuthError>.Failure(UserAuthError.EmailVerificationInvalid);
        }

        var now = _timeProvider.GetUtcNow();
        if (now > emailVerification.ExpiryAt)
        {
            _logger.LogInformation("Email verification expired.");
            _yglDbContext.EmailVerifications.Remove(emailVerification);
            await _yglDbContext.SaveChangesAsync();
            return ErrorResult<UserAuthError>.Failure(UserAuthError.EmailVerificationExpired);
        }

        var user = await _yglDbContext.Users.FirstOrDefaultAsync(x => x.Id == emailVerification.UserId);
        if (user == null)
        {
            _logger.LogInformation("User associated with email confirmation not found.");
            return ErrorResult<UserAuthError>.Failure(UserAuthError.InternalServerError);
        }

        now = _timeProvider.GetUtcNow();
        user.ExpiryAt = null;
        user.UserStatus = UserStatus.Active;
        user.ModifiedAt = now;

        _yglDbContext.EmailVerifications.Remove(emailVerification);
        await _yglDbContext.SaveChangesAsync();

        return ErrorResult<UserAuthError>.Success();
    }
}