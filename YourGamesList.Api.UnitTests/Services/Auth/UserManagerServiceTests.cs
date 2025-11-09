using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using YourGamesList.Api.Services.Auth;
using YourGamesList.Api.Services.Auth.Model;
using YourGamesList.Common;
using YourGamesList.Database;
using YourGamesList.Database.Entities;
using YourGamesList.Database.Options;
using YourGamesList.Database.TestUtils;
using YourGamesList.TestsUtils;

namespace YourGamesList.Api.UnitTests.Services.Auth;

public class UserManagerServiceTests
{
    private IFixture _fixture;
    private ILogger<UserManagerService> _logger;
    private IPasswordHasher _passwordHasher;
    private IPasswordValidator _passwordValidator;
    private ITokenProvider _tokenProvider;
    private TimeProvider _timeProvider;

    private TestYglDbContextBuilder _yglDbContextBuilder;
    private IDbContextFactory<YglDbContext> _dbContextFactory;

    private YglDbContext _yglDbContext;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _logger = Substitute.For<ILogger<UserManagerService>>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _passwordValidator = Substitute.For<IPasswordValidator>();
        _tokenProvider = Substitute.For<ITokenProvider>();
        _timeProvider = Substitute.For<TimeProvider>();

        _yglDbContextBuilder = TestYglDbContextBuilder.Build();
        _dbContextFactory = Substitute.For<IDbContextFactory<YglDbContext>>();
        _dbContextFactory.CreateDbContext().Returns(_yglDbContextBuilder.Get());
    }

    #region RegisterUser

    [Test]
    public async Task RegisterUser_SuccessfulScenario()
    {
        //ARRANGE
        var username = _fixture.Create<string>();
        var password = _fixture.Create<string>();
        var hashedPassword = _fixture.Create<HashedPassword>();
        var now = DateTime.UtcNow;
        _passwordValidator.ValidatePassword(password).Returns(ErrorResult<UserAuthError>.Clear());
        _passwordHasher.HashPassword(password).Returns(hashedPassword);
        _timeProvider.GetUtcNow().Returns(now);

        var userManagerService = new UserManagerService(_logger, _passwordHasher, _passwordValidator, _tokenProvider, _dbContextFactory, _timeProvider);

        //ACT
        var result = await userManagerService.RegisterUser(username, password);

        //ASSERT
        Assert.That(result.IsSuccess, Is.True);
        _passwordValidator.Received(1).ValidatePassword(password);
        _passwordHasher.Received(1).HashPassword(password);
        _logger.ReceivedLog(LogLevel.Information, $"User with username '{username}' registered successfully.");

        //Check if user was created in the database
        var createdUser = _yglDbContextBuilder.Get().Users.FirstOrDefault(u => u.Username == username.ToLower());
        Assert.That(createdUser, Is.Not.Null);
        Assert.That(createdUser.Username, Is.EqualTo(username.ToLower()));
        Assert.That(createdUser.PasswordHash, Is.EqualTo(hashedPassword.HashString));
        Assert.That(createdUser.Salt, Is.EqualTo(hashedPassword.Salt));
        Assert.That(createdUser.CreatedDate, Is.EqualTo((DateTimeOffset) now));
        //Check if default list was created
        var createdList = _yglDbContextBuilder.Get().Lists.FirstOrDefault(l => l.UserId == createdUser.Id && l.Name == "Played");
        Assert.That(createdList, Is.Not.Null);
        Assert.That(createdList.UserId, Is.EqualTo(createdUser.Id));
        Assert.That(createdList.CanBeDeleted, Is.False);
        Assert.That(createdList.IsPublic, Is.False);
    }

    [Test]
    public async Task RegisterUser_EmptyUsername_ReturnsInvalidUsernameError()
    {
        //ARRANGE
        var username = string.Empty;
        var password = _fixture.Create<string>();
        var userManagerService = new UserManagerService(_logger, _passwordHasher, _passwordValidator, _tokenProvider, _dbContextFactory, _timeProvider);

        //ACT
        var result = await userManagerService.RegisterUser(username, password);

        //ASSERT
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo(UserAuthError.InvalidUsername));
        _logger.ReceivedLog(LogLevel.Information, "Username is null or empty.");
    }

    [Test]
    public async Task RegisterUser_EmptyPassword_ReturnsWrongPasswordError()
    {
        //ARRANGE
        var username = _fixture.Create<string>();
        var password = string.Empty;
        var userManagerService = new UserManagerService(_logger, _passwordHasher, _passwordValidator, _tokenProvider, _dbContextFactory, _timeProvider);

        //ACT
        var result = await userManagerService.RegisterUser(username, password);

        //ASSERT
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo(UserAuthError.WrongPassword));
        _logger.ReceivedLog(LogLevel.Information, "Password is null or empty.");
    }

    [Test]
    public async Task RegisterUser_UserWithSameNameAlreadyExists_ReturnsRegisterNameAlreadyTakenError()
    {
        //ARRANGE
        var username = _fixture.Create<string>();
        var password = _fixture.Create<string>();
        var users = new List<User>()
        {
            new User()
            {
                Id = Guid.NewGuid(),
                Username = username,
                PasswordHash = _fixture.Create<string>(),
                CreatedDate = DateTime.UtcNow,
                Salt = _fixture.Create<byte[]>()
            }
        };
        _yglDbContextBuilder.WithUserDbSet(users);

        var userManagerService = new UserManagerService(_logger, _passwordHasher, _passwordValidator, _tokenProvider, _dbContextFactory, _timeProvider);

        //ACT
        var result = await userManagerService.RegisterUser(username, password);

        //ASSERT
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo(UserAuthError.RegisterNameAlreadyTaken));
        _logger.ReceivedLog(LogLevel.Information, $"User with username '{username}' already exists.");
    }

    [Test]
    public async Task RegisterUser_PasswordNotMatchingCriteria_ReturnsValidatePasswordError()
    {
        //ARRANGE
        var username = _fixture.Create<string>();
        var password = _fixture.Create<string>();
        var passwordValidationError = _fixture.Create<UserAuthError>();
        _passwordValidator.ValidatePassword(password).Returns(ErrorResult<UserAuthError>.Failure(passwordValidationError));

        var userManagerService = new UserManagerService(_logger, _passwordHasher, _passwordValidator, _tokenProvider, _dbContextFactory, _timeProvider);

        //ACT
        var result = await userManagerService.RegisterUser(username, password);

        //ASSERT
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo(passwordValidationError));
        _passwordValidator.Received(1).ValidatePassword(password);
        _logger.ReceivedLog(LogLevel.Information, $"Password for the username '{username}' is invalid. Reason: '{passwordValidationError.ToString()}'");
    }

    #endregion

    #region Login

    [Test]
    public async Task Login_SuccessfulScenario()
    {
        //ARRANGE
        var username = _fixture.Create<string>();
        var password = _fixture.Create<string>();
        var userSalt = _fixture.CreateMany<byte>().ToArray();
        var hash = _fixture.Create<string>();
        var hashedPasswordResult = new HashedPassword(hash, userSalt);
        var id = Guid.NewGuid();
        var token = _fixture.Create<string>();
        _passwordHasher.HashPassword(password, Arg.Is<byte[]>(x => x.SequenceEqual(userSalt))).Returns(hashedPasswordResult);
        _tokenProvider.CreateToken(username, id).Returns(token);
        var users = new List<User>()
        {
            new User()
            {
                Id = id,
                Username = username,
                PasswordHash = hash,
                CreatedDate = DateTime.UtcNow,
                Salt = userSalt
            }
        };
        _yglDbContextBuilder.WithUserDbSet(users);
        var userManagerService = new UserManagerService(_logger, _passwordHasher, _passwordValidator, _tokenProvider, _dbContextFactory, _timeProvider);

        //ACT
        var result = await userManagerService.Login(username, password);

        //ASSERT
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EquivalentTo(token));
        _passwordHasher.Received(1).HashPassword(password, Arg.Is<byte[]>(x => x.SequenceEqual(userSalt)));
        _tokenProvider.Received(1).CreateToken(username, id);
        _logger.ReceivedLog(LogLevel.Information, $"User with username '{username}' logged in successfully.");
    }

    [Test]
    public async Task Login_EmptyUsername_ReturnsInvalidUsernameError()
    {
        //ARRANGE
        var username = string.Empty;
        var password = _fixture.Create<string>();
        var userManagerService = new UserManagerService(_logger, _passwordHasher, _passwordValidator, _tokenProvider, _dbContextFactory, _timeProvider);

        //ACT
        var result = await userManagerService.Login(username, password);

        //ASSERT
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo(UserAuthError.InvalidUsername));
        _logger.ReceivedLog(LogLevel.Information, "Username is null or empty.");
    }

    [Test]
    public async Task Login_EmptyPassword_ReturnsWrongPasswordError()
    {
        //ARRANGE
        var username = _fixture.Create<string>();
        var password = string.Empty;
        var userManagerService = new UserManagerService(_logger, _passwordHasher, _passwordValidator, _tokenProvider, _dbContextFactory, _timeProvider);

        //ACT
        var result = await userManagerService.Login(username, password);

        //ASSERT
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo(UserAuthError.WrongPassword));
        _logger.ReceivedLog(LogLevel.Information, "Password is null or empty.");
    }

    [Test]
    public async Task Login_UserDoesNotExistsInDatabase_ReturnsNoUserFoundError()
    {
        //ARRANGE
        var username = _fixture.Create<string>();
        var password = _fixture.Create<string>();
        //Not settings up any users in the database
        var userManagerService = new UserManagerService(_logger, _passwordHasher, _passwordValidator, _tokenProvider, _dbContextFactory, _timeProvider);

        //ACT
        var result = await userManagerService.Login(username, password);

        //ASSERT
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo(UserAuthError.NoUserFound));
        _logger.ReceivedLog(LogLevel.Information, $"User with the username '{username}' was not found.");
    }

    [Test]
    public async Task Login_WrongPassword_ReturnsWrongPasswordError()
    {
        //ARRANGE
        var username = _fixture.Create<string>();
        var password = _fixture.Create<string>();
        var userSalt = _fixture.CreateMany<byte>().ToArray();
        var hash = _fixture.Create<string>();
        var differentHashInDb = _fixture.Create<string>();
        var hashedPasswordResult = new HashedPassword(hash, userSalt);
        _passwordHasher.HashPassword(password, Arg.Is<byte[]>(x => x.SequenceEqual(userSalt))).Returns(hashedPasswordResult);
        var users = new List<User>()
        {
            new User()
            {
                Id = Guid.NewGuid(),
                Username = username,
                PasswordHash = differentHashInDb,
                CreatedDate = DateTime.UtcNow,
                Salt = userSalt
            }
        };
        _yglDbContextBuilder.WithUserDbSet(users);
        var userManagerService = new UserManagerService(_logger, _passwordHasher, _passwordValidator, _tokenProvider, _dbContextFactory, _timeProvider);

        //ACT
        var result = await userManagerService.Login(username, password);

        //ASSERT
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo(UserAuthError.WrongPassword));
        _passwordHasher.Received(1).HashPassword(password, Arg.Is<byte[]>(x => x.SequenceEqual(userSalt)));
        _logger.ReceivedLog(LogLevel.Information, $"Provided password for '{username}' was invalid.");
    }

    #endregion

    #region Delete

    [Test]
    public async Task Delete_SuccessfulScenario()
    {
        //ARRANGE
        var username = _fixture.Create<string>();
        var password = _fixture.Create<string>();
        var userSalt = _fixture.CreateMany<byte>().ToArray();
        var hash = _fixture.Create<string>();
        var hashedPasswordResult = new HashedPassword(hash, userSalt);
        var id = Guid.NewGuid();
        _passwordHasher.HashPassword(password, Arg.Is<byte[]>(x => x.SequenceEqual(userSalt))).Returns(hashedPasswordResult);
        var users = new List<User>()
        {
            new User()
            {
                Id = id,
                Username = username,
                PasswordHash = hash,
                CreatedDate = DateTime.UtcNow,
                Salt = userSalt
            }
        };
        _yglDbContextBuilder.WithUserDbSet(users);
        var userManagerService = new UserManagerService(_logger, _passwordHasher, _passwordValidator, _tokenProvider, _dbContextFactory, _timeProvider);

        //ACT
        var result = await userManagerService.Delete(username, password);

        //ASSERT
        Assert.That(result.IsSuccess, Is.True);
        _passwordHasher.Received(1).HashPassword(password, Arg.Is<byte[]>(x => x.SequenceEqual(userSalt)));
        _logger.ReceivedLog(LogLevel.Information, $"User with username '{username}' removed successfully.");

        //Check if user was deleted from database
        var deletedUser = _yglDbContextBuilder.Get().Users.FirstOrDefault(u => u.Username == username.ToLower());
        Assert.That(deletedUser, Is.Null);
    }

    [Test]
    public async Task Delete_EmptyUsername_ReturnsInvalidUsernameError()
    {
        //ARRANGE
        var username = string.Empty;
        var password = _fixture.Create<string>();
        var userManagerService = new UserManagerService(_logger, _passwordHasher, _passwordValidator, _tokenProvider, _dbContextFactory, _timeProvider);

        //ACT
        var result = await userManagerService.Delete(username, password);

        //ASSERT
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo(UserAuthError.InvalidUsername));
        _logger.ReceivedLog(LogLevel.Information, "Username is null or empty.");
    }

    [Test]
    public async Task Delete_EmptyPassword_ReturnsWrongPasswordError()
    {
        //ARRANGE
        var username = _fixture.Create<string>();
        var password = string.Empty;
        var userManagerService = new UserManagerService(_logger, _passwordHasher, _passwordValidator, _tokenProvider, _dbContextFactory, _timeProvider);

        //ACT
        var result = await userManagerService.Delete(username, password);

        //ASSERT
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo(UserAuthError.WrongPassword));
        _logger.ReceivedLog(LogLevel.Information, "Password is null or empty.");
    }

    [Test]
    public async Task Delete_UserDoesNotExistsInDatabase_ReturnsNoUserFoundError()
    {
        //ARRANGE
        var username = _fixture.Create<string>();
        var password = _fixture.Create<string>();
        //Not settings up any users in the database
        var userManagerService = new UserManagerService(_logger, _passwordHasher, _passwordValidator, _tokenProvider, _dbContextFactory, _timeProvider);

        //ACT
        var result = await userManagerService.Delete(username, password);

        //ASSERT
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo(UserAuthError.NoUserFound));
        _logger.ReceivedLog(LogLevel.Information, $"User with the username '{username}' was not found.");
    }

    [Test]
    public async Task Delete_WrongPassword_ReturnsWrongPasswordError()
    {
        //ARRANGE
        var username = _fixture.Create<string>();
        var password = _fixture.Create<string>();
        var userSalt = _fixture.CreateMany<byte>().ToArray();
        var hash = _fixture.Create<string>();
        var differentHashInDb = _fixture.Create<string>();
        var hashedPasswordResult = new HashedPassword(hash, userSalt);
        _passwordHasher.HashPassword(password, Arg.Is<byte[]>(x => x.SequenceEqual(userSalt))).Returns(hashedPasswordResult);
        var users = new List<User>()
        {
            new User()
            {
                Id = Guid.NewGuid(),
                Username = username,
                PasswordHash = differentHashInDb,
                CreatedDate = DateTime.UtcNow,
                Salt = userSalt
            }
        };
        _yglDbContextBuilder.WithUserDbSet(users);
        var userManagerService = new UserManagerService(_logger, _passwordHasher, _passwordValidator, _tokenProvider, _dbContextFactory, _timeProvider);

        //ACT
        var result = await userManagerService.Delete(username, password);

        //ASSERT
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo(UserAuthError.WrongPassword));
        _passwordHasher.Received(1).HashPassword(password, Arg.Is<byte[]>(x => x.SequenceEqual(userSalt)));
        _logger.ReceivedLog(LogLevel.Information, $"Provided password for '{username}' was invalid.");
    }

    #endregion
}