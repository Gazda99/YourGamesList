using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YourGamesList.Api.Services.Auth.Model;
using YourGamesList.Api.Services.Auth.Options;
using YourGamesList.Api.Services.Lists;
using YourGamesList.Common;
using YourGamesList.Database;
using YourGamesList.Database.Entity;

namespace YourGamesList.Api.Services.Auth;

public interface IUserManagerService
{
    /// <summary>
    /// Registers a user.
    /// </summary>
    Task<CombinedResult<Guid, UserAuthError>> Register(UserRegisterParams userRegisterParams);

    /// <summary>
    /// Authenticates a user and returns a token.
    /// </summary>
    Task<CombinedResult<string, UserAuthError>> Login(UserLoginParams userLoginParams);

    /// <summary>
    /// Deletes a user account.
    /// </summary>
    Task<ErrorResult<UserAuthError>> Delete(UserDeleteParams userDeleteParams);

    /// <summary>
    /// Confirms a user's email address.
    /// </summary>
    Task<ErrorResult<UserAuthError>> ConfirmEmail(ConfirmEmailParams confirmEmailParams);
}

public class UserManagerService : IUserManagerService
{
    private readonly ILogger<UserManagerService> _logger;
    private readonly IOptions<UserManagerServiceOptions> _options;
    private readonly IPasswordHasher _passwordHasher;
    private readonly TimeProvider _timeProvider;
    private readonly IVerificationTokenHasher _verificationTokenHasher;
    private readonly IVerificationEmailSender _verificationEmailSender;
    private readonly ITokenProvider _tokenProvider;
    private readonly IDefaultListsCreator _defaultListsCreator;
    private readonly YourGamesListDbContext _yglDbContext;

    public UserManagerService(ILogger<UserManagerService> logger,
        IOptions<UserManagerServiceOptions> options,
        YourGamesListDbContext yglDbContext,
        IPasswordHasher passwordHasher,
        TimeProvider timeProvider,
        IVerificationTokenHasher verificationTokenHasher,
        IVerificationEmailSender verificationEmailSender,
        ITokenProvider tokenProvider,
        IDefaultListsCreator defaultListsCreator
    )
    {
        _logger = logger;
        _options = options;
        _passwordHasher = passwordHasher;
        _timeProvider = timeProvider;
        _verificationTokenHasher = verificationTokenHasher;
        _verificationEmailSender = verificationEmailSender;
        _tokenProvider = tokenProvider;
        _defaultListsCreator = defaultListsCreator;
        _yglDbContext = yglDbContext;
    }

    public async Task<CombinedResult<string, UserAuthError>> Login(UserLoginParams userLoginParams)
    {
        var normalizedUsername = userLoginParams.Username.ToLowerInvariant();
        var user = await _yglDbContext.Users.FirstOrDefaultAsync(u => u.NormalizedUsername == normalizedUsername);

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
        var passwordMatch = _passwordHasher.VerifyHashedPassword(userLoginParams.Password, hashedPassword);

        if (!passwordMatch)
        {
            _logger.LogInformation("Password does not match.");
            return CombinedResult<string, UserAuthError>.Failure(UserAuthError.UnauthorizedUser);
        }

        var newToken = _tokenProvider.CreateToken(user.NormalizedUsername, user.Id);
        _logger.LogInformation("Created new user token. User logged in.");

        var now = _timeProvider.GetUtcNow();
        user.LastLoginDate = now;
        await _yglDbContext.SaveChangesAsync();

        return CombinedResult<string, UserAuthError>.Success(newToken);
    }

    public async Task<CombinedResult<Guid, UserAuthError>> Register(UserRegisterParams userRegisterParams)
    {
        var username = userRegisterParams.Username;
        var email = userRegisterParams.Email;
        var dob = userRegisterParams.DateOfBirth;

        var normalizedUsername = username.ToLowerInvariant();
        List<User> conflictingUsers;

        try
        {
            conflictingUsers = await _yglDbContext.Users
                .Where(u => u.Email == email || u.NormalizedUsername == normalizedUsername)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Register failed while checking username and email availability in db.");
            return CombinedResult<Guid, UserAuthError>.Failure(UserAuthError.InternalServerError);
        }

        var checkTime = _timeProvider.GetUtcNow();
        var expiredPendingUsers = conflictingUsers.Where(u => IsExpiredPendingUser(u, checkTime)).ToList();
        var activeConflicts = conflictingUsers.Except(expiredPendingUsers).ToList();

        if (activeConflicts.Any(u => u.Email == email))
        {
            _logger.LogInformation("Email already registered.");
            return CombinedResult<Guid, UserAuthError>.Failure(UserAuthError.EmailAlreadyTaken);
        }

        if (activeConflicts.Any(u => u.NormalizedUsername == normalizedUsername))
        {
            _logger.LogInformation("Username already registered.");
            return CombinedResult<Guid, UserAuthError>.Failure(UserAuthError.UsernameAlreadyTaken);
        }

        if (expiredPendingUsers.Count > 0)
        {
            _logger.LogInformation("Replacing {ExpiredPendingUsersCount} expired pending user(s).", expiredPendingUsers.Count);
            _yglDbContext.Users.RemoveRange(expiredPendingUsers);
        }

        var hashedPassword = _passwordHasher.HashPassword(userRegisterParams.Password);
        if (hashedPassword.IsFailure)
        {
            _logger.LogError("Hashing of password failed.");
            return CombinedResult<Guid, UserAuthError>.Failure(UserAuthError.InternalServerError);
        }

        var now = _timeProvider.GetUtcNow();

        User user;
        (string RawToken, string TokenHash)? emailVerificationToken = null;
        if (!_options.Value.EnableEmailConfirmation)
        {
            _logger.LogInformation("Email confirmation is disabled. Proceeding with adding the User with status {UserStatus}", nameof(UserStatus.Active));

            user = new User()
            {
                CreatedAt = now,
                DateOfBirth = dob,
                Email = email,
                Username = username,
                NormalizedUsername = normalizedUsername,
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
                NormalizedUsername = normalizedUsername,
                PasswordHash = hashedPassword.Value,
                UserStatus = UserStatus.Pending,
                ExpiryAt = userExpiration
            };

            _yglDbContext.Users.Add(user);

            emailVerificationToken = _verificationTokenHasher.GenerateToken();
            var emailVerificationExpiration = now + TimeSpan.FromMinutes(_options.Value.DefaultEmailVerificationExpiryInMinutes);
            var emailVerification = new EmailVerification()
            {
                CreatedAt = now,
                ExpiryAt = emailVerificationExpiration,
                Token = emailVerificationToken.Value.TokenHash,
                User = user
            };

            _yglDbContext.EmailVerifications.Add(emailVerification);
        }

        await _defaultListsCreator.AddDefaultListsToGivenUser(_yglDbContext, user);
        await _yglDbContext.SaveChangesAsync();
        _logger.LogInformation("Added user to database");

        // Email is sent only after the user is persisted, so the link always points to an existing verification.
        if (emailVerificationToken.HasValue)
        {
            var wasEmailSent = await _verificationEmailSender.SendVerificationEmail(email, username, emailVerificationToken.Value.RawToken);
            if (!wasEmailSent)
            {
                _logger.LogWarning("Email verification was not sent!");
                await RemoveUserAfterFailedEmail(user);
                return CombinedResult<Guid, UserAuthError>.Failure(UserAuthError.InternalServerError);
            }

            _logger.LogInformation("Email verification sent!");
        }

        return CombinedResult<Guid, UserAuthError>.Success(user.Id);
    }

    private async Task RemoveUserAfterFailedEmail(User user)
    {
        try
        {
            // Db cascades remove the email verification and default lists.
            _yglDbContext.Users.Remove(user);
            await _yglDbContext.SaveChangesAsync();
            _logger.LogInformation("Removed user from database after failed email verification sending.");
        }
        catch (Exception ex)
        {
            // User stays Pending with ExpiryAt, so it becomes replaceable by Register once expired.
            _logger.LogError(ex, "Failed to remove user from database after failed email verification sending.");
        }
    }

    public async Task<ErrorResult<UserAuthError>> Delete(UserDeleteParams userDeleteParams)
    {
        var normalizedUsername = userDeleteParams.Username.ToLowerInvariant();
        var userId = userDeleteParams.UserInformation.UserId;
        var user = await _yglDbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            _logger.LogInformation("Not user found.");
            return ErrorResult<UserAuthError>.Failure(UserAuthError.NoUserFound);
        }

        if (normalizedUsername != user.NormalizedUsername)
        {
            _logger.LogInformation("Requested username does not match authenticated user.");
            return ErrorResult<UserAuthError>.Failure(UserAuthError.ForbiddenUser);
        }

        var hashedPassword = user.PasswordHash;
        var passwordMatch = _passwordHasher.VerifyHashedPassword(userDeleteParams.Password, hashedPassword);

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

    public async Task<ErrorResult<UserAuthError>> ConfirmEmail(ConfirmEmailParams confirmEmailParams)
    {
        if (string.IsNullOrWhiteSpace(confirmEmailParams.Token))
        {
            _logger.LogInformation("Email verification token is empty.");
            return ErrorResult<UserAuthError>.Failure(UserAuthError.EmailVerificationInvalid);
        }

        var token = confirmEmailParams.Token.ToLower();
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
        if (IsExpiredPendingUser(user, now))
        {
            _logger.LogInformation("User associated with email confirmation has expired.");
            return ErrorResult<UserAuthError>.Failure(UserAuthError.EmailVerificationExpired);
        }

        user.ExpiryAt = null;
        user.UserStatus = UserStatus.Active;
        user.ModifiedAt = now;

        _yglDbContext.EmailVerifications.Remove(emailVerification);
        await _yglDbContext.SaveChangesAsync();
        _logger.LogInformation("Email confirmed. User {UserId} is now {UserStatus}.", user.Id, nameof(UserStatus.Active));

        return ErrorResult<UserAuthError>.Success();
    }

    private static bool IsExpiredPendingUser(User user, DateTimeOffset now)
    {
        return user.UserStatus == UserStatus.Pending && user.ExpiryAt.HasValue && user.ExpiryAt.Value <= now;
    }
}