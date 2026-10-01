using System;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using YourGamesList.Api.Services.Auth;
using YourGamesList.Api.Services.Auth.Model;
using YourGamesList.Api.Services.Auth.Options;
using YourGamesList.Common;
using YourGamesList.Database;
using YourGamesList.Database.Entity;
using YourGamesList.TestsUtils;

namespace YourGamesList.Api.UnitTests.Services.Auth;

public class UserManagerServiceTests
{
    private IFixture _fixture;
    private ILogger<UserManagerService> _logger;
    private IOptions<UserManagerServiceOptions> _options;
    private IUserValuesValidator _userValuesValidator;
    private IDbContextFactory<YourGamesListDbContext> _dbContextFactory;
    private IPasswordHasher _passwordHasher;
    private FakeTimeProvider _timeProvider;
    private IVerificationTokenHasher _verificationTokenHasher;
    private IVerificationEmailSender _verificationEmailSender;
    private ITokenProvider _tokenProvider;
    private YourGamesListDbContext _dbContext;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _fixture.Customize<DateOnly>(composer => composer.FromFactory(() => DateOnly.FromDateTime(_fixture.Create<DateTime>())));
        _logger = Substitute.For<ILogger<UserManagerService>>();
        _options = Substitute.For<IOptions<UserManagerServiceOptions>>();
        _userValuesValidator = Substitute.For<IUserValuesValidator>();
        _dbContextFactory = Substitute.For<IDbContextFactory<YourGamesListDbContext>>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _timeProvider = new FakeTimeProvider();
        _verificationTokenHasher = Substitute.For<IVerificationTokenHasher>();
        _verificationEmailSender = Substitute.For<IVerificationEmailSender>();
        _tokenProvider = Substitute.For<ITokenProvider>();

        var optionsBuilder = new DbContextOptionsBuilder<YourGamesListDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString());

        _dbContext = new YourGamesListDbContext(optionsBuilder.Options);
        _dbContextFactory.CreateDbContext().Returns(_dbContext);

        _options.Value.Returns(new UserManagerServiceOptions
        {
            EnableEmailConfirmation = false,
            DefaultPendingUserExpiryInMinutes = 1440,
            DefaultEmailVerificationExpiryInMinutes = 1440
        });

        _userValuesValidator.ValidateEmail(Arg.Any<string>()).Returns(ErrorResult<UserAuthError>.Success());
        _userValuesValidator.ValidateUsername(Arg.Any<string>()).Returns(ErrorResult<UserAuthError>.Success());
        _userValuesValidator.ValidatePassword(Arg.Any<string>()).Returns(ErrorResult<UserAuthError>.Success());
        _userValuesValidator.ValidateDateOfBirth(Arg.Any<DateOnly>()).Returns(ErrorResult<UserAuthError>.Success());
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    #region Login

    [Test]
    public async Task Login_UserNotFound_ReturnsNoUserFoundError()
    {
        //ARRANGE
        var dto = _fixture.Create<UserLoginDto>();
        var service = CreateService();

        //ACT
        var res = await service.Login(dto);

        //ASSERT
        Assert.That(res.IsSuccess, Is.False);
        Assert.That(res.Error, Is.EqualTo(UserAuthError.NoUserFound));
        _logger.ReceivedLog(LogLevel.Information, "Not user found.");
    }

    [Test]
    public async Task Login_UserStatusPending_ReturnsUserNotActiveEmailActivationError()
    {
        //ARRANGE
        const string username = "testuser";
        var user = CreateUser(username: username, status: UserStatus.Pending);
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var dto = new UserLoginDto()
        {
            Username = username,
            Password = "password123"
        };
        var service = CreateService();

        //ACT
        var res = await service.Login(dto);

        //ASSERT
        Assert.That(res.IsSuccess, Is.False);
        Assert.That(res.Error, Is.EqualTo(UserAuthError.UserNotActiveEmailActivation));
        _logger.ReceivedLog(LogLevel.Information, $"User status is {nameof(UserStatus.Pending)}.");
    }

    [Test]
    public async Task Login_InvalidPassword_ReturnsUnauthorizedUserError()
    {
        //ARRANGE
        const string username = "testuser";
        const string password = "wrong_password";
        const string hashedPassword = "hashed_password";

        var user = CreateUser(username: username, passwordHash: hashedPassword, status: UserStatus.Active);
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        _passwordHasher.VerifyHashedPassword(password, hashedPassword).Returns(false);

        var dto = new UserLoginDto()
        {
            Username = username,
            Password = password
        };
        var service = CreateService();

        //ACT
        var res = await service.Login(dto);

        //ASSERT
        Assert.That(res.IsSuccess, Is.False);
        Assert.That(res.Error, Is.EqualTo(UserAuthError.UnauthorizedUser));
        _logger.ReceivedLog(LogLevel.Information, "Password does not match.");
    }

    [Test]
    public async Task Login_ValidCredentials_ReturnsTokenAndUpdateLastLoginDate()
    {
        //ARRANGE
        var now = DateTimeOffset.UtcNow;
        _timeProvider.SetUtcNow(now);

        const string username = "testuser";
        const string password = "correct_password";
        const string hashedPassword = "hashed_password";
        const string expectedToken = "jwt_token_sample";

        var userId = Guid.NewGuid();
        var user = CreateUser(id: userId, username: username, passwordHash: hashedPassword, status: UserStatus.Active);
        user.LastLoginDate = null;

        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        _passwordHasher.VerifyHashedPassword(password, hashedPassword).Returns(true);
        _tokenProvider.CreateToken(username, userId).Returns(expectedToken);

        var dto = new UserLoginDto()
        {
            Username = username,
            Password = password
        };
        var service = CreateService();

        //ACT
        var res = await service.Login(dto);

        //ASSERT
        Assert.That(res.IsSuccess, Is.True);
        Assert.That(res.Value, Is.EqualTo(expectedToken));

        var dbUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        Assert.That(dbUser, Is.Not.Null);
        Assert.That(dbUser.LastLoginDate, Is.EqualTo(now));
        _logger.ReceivedLog(LogLevel.Information, "Created new user token. User logged in.");
    }

    #endregion

    #region Register

    [Test]
    public async Task Register_InvalidEmail_ReturnsEmailError()
    {
        //ARRANGE
        const UserAuthError error = UserAuthError.InvalidEmail;
        var dto = _fixture.Create<UserRegisterDto>();
        _userValuesValidator.ValidateEmail(Arg.Any<string>()).Returns(ErrorResult<UserAuthError>.Failure(error));

        var service = CreateService();

        //ACT
        var res = await service.Register(dto);

        //ASSERT
        Assert.That(res.IsSuccess, Is.False);
        Assert.That(res.Error, Is.EqualTo(error));
        _logger.ReceivedLog(LogLevel.Information,  $"Cannot register user due to email being invalid. Error: {error.ToString()}");
    }

    [Test]
    public async Task Register_InvalidUsername_ReturnsUsernameError()
    {
        //ARRANGE
        const UserAuthError error = UserAuthError.UsernameIsTooShort;
        var dto = _fixture.Create<UserRegisterDto>();
        _userValuesValidator.ValidateUsername(Arg.Any<string>()).Returns(ErrorResult<UserAuthError>.Failure(error));

        var service = CreateService();

        //ACT
        var res = await service.Register(dto);

        //ASSERT
        Assert.That(res.IsSuccess, Is.False);
        Assert.That(res.Error, Is.EqualTo(error));
        _logger.ReceivedLog(LogLevel.Information,  $"Cannot register user due to username being invalid. Error: {error.ToString()}");
    }

    [Test]
    public async Task Register_InvalidPassword_ReturnsPasswordError()
    {
        //ARRANGE
        const UserAuthError error = UserAuthError.PasswordIsTooShort;
        var dto = _fixture.Create<UserRegisterDto>();
        _userValuesValidator.ValidatePassword(Arg.Any<string>()).Returns(ErrorResult<UserAuthError>.Failure(error));

        var service = CreateService();

        //ACT
        var res = await service.Register(dto);

        //ASSERT
        Assert.That(res.IsSuccess, Is.False);
        Assert.That(res.Error, Is.EqualTo(error));
        _logger.ReceivedLog(LogLevel.Information,  $"Cannot register user due to password being invalid. Error: {error.ToString()}");
    }

    [Test]
    public async Task Register_InvalidDateOfBirth_ReturnsDateOfBirthError()
    {
        //ARRANGE
        const UserAuthError error = UserAuthError.UnderMinimumAge;
        var dto = _fixture.Create<UserRegisterDto>();
        _userValuesValidator.ValidateDateOfBirth(Arg.Any<DateOnly>()).Returns(ErrorResult<UserAuthError>.Failure(error));

        var service = CreateService();

        //ACT
        var res = await service.Register(dto);

        //ASSERT
        Assert.That(res.IsSuccess, Is.False);
        Assert.That(res.Error, Is.EqualTo(error));
        _logger.ReceivedLog(LogLevel.Information,  $"Cannot register user due to date of birth being invalid. Error: {error.ToString()}");
    }

    [Test]
    public async Task Register_EmailAlreadyExists_ReturnsEmailAlreadyTaken()
    {
        //ARRANGE
        const string email = "existing@example.com";
        await _dbContext.Users.AddAsync(CreateUser(username: "otheruser", email: email));
        await _dbContext.SaveChangesAsync();

        var dto = new UserRegisterDto()
        {
            Username = "newUser",
            Email = email,
            Password = "password123",
            DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20))
        };
        var service = CreateService();

        //ACT
        var res = await service.Register(dto);

        //ASSERT
        Assert.That(res.IsSuccess, Is.False);
        Assert.That(res.Error, Is.EqualTo(UserAuthError.EmailAlreadyTaken));
        _logger.ReceivedLog(LogLevel.Information, "Email already registered.");
    }

    [Test]
    public async Task Register_UsernameAlreadyExists_ReturnsUsernameAlreadyTaken()
    {
        //ARRANGE
        const string username = "existinguser";
        await _dbContext.Users.AddAsync(CreateUser(username: username, email: "other@example.com"));
        await _dbContext.SaveChangesAsync();

        var dto = new UserRegisterDto()
        {
            Username = username,
            Email = "new@example.com",
            Password = "password123",
            DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20))
        };
        var service = CreateService();

        //ACT
        var res = await service.Register(dto);

        //ASSERT
        Assert.That(res.IsSuccess, Is.False);
        Assert.That(res.Error, Is.EqualTo(UserAuthError.UsernameAlreadyTaken));
        _logger.ReceivedLog(LogLevel.Information, "Username already registered.");
    }

    [Test]
    public async Task Register_PasswordHashingFails_ReturnsInternalServerError()
    {
        //ARRANGE
        var dto = _fixture.Create<UserRegisterDto>();
        _passwordHasher.HashPassword(Arg.Any<string>()).Returns(ValueResult<string>.Failure());

        var service = CreateService();

        //ACT
        var res = await service.Register(dto);

        //ASSERT
        Assert.That(res.IsSuccess, Is.False);
        Assert.That(res.Error, Is.EqualTo(UserAuthError.InternalServerError));
        _logger.ReceivedLog(LogLevel.Error, "Hashing of password failed.");
    }

    [Test]
    public async Task Register_EmailConfirmationDisabled_CreatesActiveUserAndReturnsSuccess()
    {
        //ARRANGE
        var now = DateTimeOffset.UtcNow;
        _timeProvider.SetUtcNow(now);

        _options.Value.Returns(new UserManagerServiceOptions
        {
            EnableEmailConfirmation = false
        });

        _passwordHasher.HashPassword(Arg.Any<string>()).Returns(ValueResult<string>.Success("hashed_password"));

        var dto = new UserRegisterDto()
        {
            Username = "newuser",
            Email = "new@example.com",
            Password = "password123",
            DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20))
        };
        var service = CreateService();

        //ACT
        var res = await service.Register(dto);

        //ASSERT
        Assert.That(res.IsSuccess, Is.True);

        var dbUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == "newuser");
        Assert.That(dbUser, Is.Not.Null);
        Assert.That(dbUser.UserStatus, Is.EqualTo(UserStatus.Active));
        Assert.That(dbUser.CreatedAt, Is.EqualTo(now));
        Assert.That(dbUser.ExpiryAt, Is.Null);
        
        _logger.ReceivedLog(LogLevel.Information, $"Email confirmation is disabled. Proceeding with adding the User with status {nameof(UserStatus.Active)}");
        _logger.ReceivedLog(LogLevel.Information, "Added user to database");
    }

    [Test]
    public async Task Register_EmailConfirmationEnabled_EmailSendingFails_ReturnsInternalServerError()
    {
        //ARRANGE
        _options.Value.Returns(new UserManagerServiceOptions
        {
            EnableEmailConfirmation = true,
            DefaultPendingUserExpiryInMinutes = 60
        });

        _passwordHasher.HashPassword(Arg.Any<string>()).Returns(ValueResult<string>.Success("hashed_password"));
        _verificationTokenHasher.GenerateToken().Returns(("raw_token", "hashed_token"));
        _verificationEmailSender.SendVerificationEmail(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(Task.FromResult(false));

        var dto = new UserRegisterDto()
        {
            Username = "newuser",
            Email = "new@example.com",
            Password = "password123",
            DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20))
        };
        var service = CreateService();

        //ACT
        var res = await service.Register(dto);

        //ASSERT
        Assert.That(res.IsSuccess, Is.False);
        Assert.That(res.Error, Is.EqualTo(UserAuthError.InternalServerError));

        var dbUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == "newuser");
        Assert.That(dbUser, Is.Null);

        _logger.ReceivedLog(LogLevel.Information, $"Email confirmation is enabled. Proceeding with adding the User with status {nameof(UserStatus.Pending)}");
        _logger.ReceivedLog(LogLevel.Warning, "Email verification was not sent!");
    }

    [Test]
    public async Task Register_EmailConfirmationEnabled_EmailSendingSucceeds_CreatesPendingUserAndVerification()
    {
        //ARRANGE
        var now = DateTimeOffset.UtcNow;
        var username = "newuser2137";
        var password = "password123";
        var email = "email@com.pl";
        var rawToken = _fixture.Create<string>();
        _timeProvider.SetUtcNow(now);

        _options.Value.Returns(new UserManagerServiceOptions
        {
            EnableEmailConfirmation = true,
            DefaultPendingUserExpiryInMinutes = 60,
            DefaultEmailVerificationExpiryInMinutes = 30
        });

        _passwordHasher.HashPassword(password).Returns(ValueResult<string>.Success("hashed_password"));
        _verificationTokenHasher.GenerateToken().Returns((rawToken, "hashed_token_123"));
        _verificationEmailSender.SendVerificationEmail(email, username, rawToken).Returns(Task.FromResult(true));

        var dto = new UserRegisterDto()
        {
            Username = username,
            Email = email,
            Password = password,
            DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20))
        };
        var service = CreateService();

        //ACT
        var res = await service.Register(dto);

        //ASSERT
        Assert.That(res.IsSuccess, Is.True);
        var dbUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
        Assert.That(dbUser, Is.Not.Null);
        
        Assert.That(res.Value, Is.EqualTo(dbUser.Id));

        Assert.That(dbUser.UserStatus, Is.EqualTo(UserStatus.Pending));
        Assert.That(dbUser.ExpiryAt, Is.EqualTo(now + TimeSpan.FromMinutes(60)));

        var dbVerification = await _dbContext.EmailVerifications.FirstOrDefaultAsync(ev => ev.UserId == dbUser.Id);
        Assert.That(dbVerification, Is.Not.Null);
        Assert.That(dbVerification.Token, Is.EqualTo("hashed_token_123"));
        Assert.That(dbVerification.ExpiryAt, Is.EqualTo(now + TimeSpan.FromMinutes(30)));

        _logger.ReceivedLog(LogLevel.Information, $"Email confirmation is enabled. Proceeding with adding the User with status {nameof(UserStatus.Pending)}");
        _logger.ReceivedLog(LogLevel.Information, "Email verification sent!");
        _logger.ReceivedLog(LogLevel.Information, "Added user to database");
    }

    #endregion

    #region Delete

    [Test]
    public async Task Delete_UserNotFound_ReturnsNoUserFoundError()
    {
        //ARRANGE
        var dto = _fixture.Create<UserDeleteDto>();
        var service = CreateService();

        //ACT
        var res = await service.Delete(dto);

        //ASSERT
        Assert.That(res.IsSuccess, Is.False);
        Assert.That(res.Error, Is.EqualTo(UserAuthError.NoUserFound));
        _logger.ReceivedLog(LogLevel.Information, "Not user found.");
    }

    [Test]
    public async Task Delete_InvalidPassword_ReturnsUnauthorizedUserError()
    {
        //ARRANGE
        const string username = "user_to_delete";
        const string password = "wrong_password";
        const string hashedPassword = "hashed_password";

        var user = CreateUser(username: username, passwordHash: hashedPassword);
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        _passwordHasher.VerifyHashedPassword(password, hashedPassword).Returns(false);

        var dto = new UserDeleteDto()
        {
            Username = username,
            Password = password
        };
        var service = CreateService();

        //ACT
        var res = await service.Delete(dto);

        //ASSERT
        Assert.That(res.IsSuccess, Is.False);
        Assert.That(res.Error, Is.EqualTo(UserAuthError.UnauthorizedUser));
        _logger.ReceivedLog(LogLevel.Information, "Password does not match.");
    }

    [Test]
    public async Task Delete_ValidUserAndPassword_RemovesUserAndReturnsSuccess()
    {
        //ARRANGE
        const string username = "user_to_delete";
        const string password = "correct_password";
        const string hashedPassword = "hashed_password";

        var user = CreateUser(username: username, passwordHash: hashedPassword);
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        _passwordHasher.VerifyHashedPassword(password, hashedPassword).Returns(true);

        var dto = new UserDeleteDto()
        {
            Username = username,
            Password = password
        };
        var service = CreateService();

        //ACT
        var res = await service.Delete(dto);

        //ASSERT
        Assert.That(res.IsSuccess, Is.True);

        var dbUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
        Assert.That(dbUser, Is.Null);
        _logger.ReceivedLog(LogLevel.Information, "Removed user from database.");
    }

    #endregion

    #region ConfirmEmail

    [Test]
    public async Task ConfirmEmail_TokenNotFound_ReturnsEmailVerificationInvalid()
    {
        //ARRANGE
        const string rawToken = "invalid_token";
        const string hashedToken = "hashed_invalid_token";

        _verificationTokenHasher.HashToken(rawToken.ToLower()).Returns(hashedToken);

        var dto = new ConfirmEmailDto()
        {
            Token = rawToken
        };
        var service = CreateService();

        //ACT
        var res = await service.ConfirmEmail(dto);

        //ASSERT
        Assert.That(res.IsSuccess, Is.False);
        Assert.That(res.Error, Is.EqualTo(UserAuthError.EmailVerificationInvalid));
        _logger.ReceivedLog(LogLevel.Information, "Email verification not found.");
    }

    [Test]
    public async Task ConfirmEmail_TokenExpired_RemovesTokenAndReturnsEmailVerificationExpired()
    {
        //ARRANGE
        var now = DateTimeOffset.UtcNow;
        _timeProvider.SetUtcNow(now);

        const string rawToken = "expired_token";
        const string hashedToken = "hashed_expired_token";

        _verificationTokenHasher.HashToken(rawToken.ToLower()).Returns(hashedToken);

        var emailVerification = new EmailVerification
        {
            CreatedAt = now,
            Id = Guid.NewGuid(),
            Token = hashedToken,
            ExpiryAt = now.AddMinutes(-5),
            UserId = Guid.NewGuid()
        };
        await _dbContext.EmailVerifications.AddAsync(emailVerification);
        await _dbContext.SaveChangesAsync();

        var dto = new ConfirmEmailDto()
        {
            Token = rawToken
        };
        var service = CreateService();

        //ACT
        var res = await service.ConfirmEmail(dto);

        //ASSERT
        Assert.That(res.IsSuccess, Is.False);
        Assert.That(res.Error, Is.EqualTo(UserAuthError.EmailVerificationExpired));

        var dbVerification = await _dbContext.EmailVerifications.FirstOrDefaultAsync(x => x.Token == hashedToken);
        Assert.That(dbVerification, Is.Null);
        _logger.ReceivedLog(LogLevel.Information, "Email verification expired.");
    }

    [Test]
    public async Task ConfirmEmail_UserNotFound_ReturnsInternalServerError()
    {
        //ARRANGE
        var now = DateTimeOffset.UtcNow;
        _timeProvider.SetUtcNow(now);

        const string rawToken = "valid_token";
        const string hashedToken = "hashed_valid_token";

        _verificationTokenHasher.HashToken(rawToken.ToLower()).Returns(hashedToken);

        var emailVerification = new EmailVerification
        {
            CreatedAt = now,
            Id = Guid.NewGuid(),
            Token = hashedToken,
            ExpiryAt = now.AddMinutes(30),
            UserId = Guid.NewGuid()
        };
        await _dbContext.EmailVerifications.AddAsync(emailVerification);
        await _dbContext.SaveChangesAsync();

        var dto = new ConfirmEmailDto()
        {
            Token = rawToken
        };
        var service = CreateService();

        //ACT
        var res = await service.ConfirmEmail(dto);

        //ASSERT
        Assert.That(res.IsSuccess, Is.False);
        Assert.That(res.Error, Is.EqualTo(UserAuthError.InternalServerError));
        _logger.ReceivedLog(LogLevel.Information, "User associated with email confirmation not found.");
    }

    [Test]
    public async Task ConfirmEmail_ValidToken_ActivatesUserRemovesVerificationAndReturnsSuccess()
    {
        //ARRANGE
        var now = DateTimeOffset.UtcNow;
        _timeProvider.SetUtcNow(now);

        const string rawToken = "valid_token";
        const string hashedToken = "hashed_valid_token";

        _verificationTokenHasher.HashToken(rawToken.ToLower()).Returns(hashedToken);

        var user = CreateUser(username: "pendinguser", status: UserStatus.Pending);
        user.ExpiryAt = now.AddHours(24);
        await _dbContext.Users.AddAsync(user);

        var emailVerification = new EmailVerification
        {
            CreatedAt = now,
            Id = Guid.NewGuid(),
            Token = hashedToken,
            ExpiryAt = now.AddMinutes(30),
            UserId = user.Id
        };
        await _dbContext.EmailVerifications.AddAsync(emailVerification);
        await _dbContext.SaveChangesAsync();

        var dto = new ConfirmEmailDto()
        {
            Token = rawToken
        };
        var service = CreateService();

        //ACT
        var res = await service.ConfirmEmail(dto);

        //ASSERT
        Assert.That(res.IsSuccess, Is.True);

        var dbUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        Assert.That(dbUser, Is.Not.Null);
        Assert.That(dbUser.UserStatus, Is.EqualTo(UserStatus.Active));
        Assert.That(dbUser.ExpiryAt, Is.Null);
        Assert.That(dbUser.ModifiedAt, Is.EqualTo(now));

        var dbVerification = await _dbContext.EmailVerifications.FirstOrDefaultAsync(x => x.Token == hashedToken);
        Assert.That(dbVerification, Is.Null);
    }

    #endregion

    private static User CreateUser(
        Guid? id = null,
        string username = "testuser",
        string email = "test@example.com",
        string passwordHash = "hashed_password",
        UserStatus status = UserStatus.Active)
    {
        return new User
        {
            Id = id ?? Guid.NewGuid(),
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            CreatedAt = DateTimeOffset.UtcNow,
            DateOfBirth = new DateOnly(2000, 1, 1),
            UserStatus = status
        };
    }

    private UserManagerService CreateService()
    {
        return new UserManagerService(
            _logger,
            _options,
            _userValuesValidator,
            _dbContextFactory,
            _passwordHasher,
            _timeProvider,
            _verificationTokenHasher,
            _verificationEmailSender,
            _tokenProvider
        );
    }
}