using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using YourGamesList.Api.Model;
using YourGamesList.Api.Services.YourGamesList;
using YourGamesList.Api.Services.YourGamesList.Model;
using YourGamesList.Database;
using YourGamesList.Database.Entity;
using YourGamesList.TestsUtils;

namespace YourGamesList.Api.UnitTests.Services.YourGamesList;

public class UsersServiceTests
{
    private ILogger<UsersService> _logger = null!;
    private IDbContextFactory<YourGamesListDbContext> _dbContextFactory = null!;
    private YourGamesListDbContext _dbContext = null!;

    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger<UsersService>>();
        _dbContextFactory = Substitute.For<IDbContextFactory<YourGamesListDbContext>>();

        var options = new DbContextOptionsBuilder<YourGamesListDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new YourGamesListDbContext(options);
        _dbContextFactory.CreateDbContext().Returns(_dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Test]
    public async Task GetSelfUser_UserExists_ReturnsMappedUser()
    {
        var userId = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow;
        var dateOfBirth = new DateOnly(1995, 3, 14);
        const string username = "testuser";
        const string description = "A test description";
        _dbContext.Users.Add(new User
        {
            Id = userId,
            CreatedAt = createdAt,
            DateOfBirth = dateOfBirth,
            Description = description,
            Email = "test@example.com",
            PasswordHash = "password-hash",
            UserStatus = UserStatus.Active,
            Username = username
        });
        await _dbContext.SaveChangesAsync();

        var service = CreateService();
        var parameters = new UserGetSelfParams
        {
            UserInformation = CreateUserInformation(userId)
        };

        var result = await service.GetSelfUser(parameters);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Id, Is.EqualTo(userId));
        Assert.That(result.Value.CreatedAt, Is.EqualTo(createdAt));
        Assert.That(result.Value.Username, Is.EqualTo(username));
        Assert.That(result.Value.Description, Is.EqualTo(description));
        Assert.That(result.Value.DateOfBirth, Is.EqualTo(dateOfBirth));
    }

    [Test]
    public async Task GetSelfUser_UserDoesNotExist_ReturnsUserNotFound()
    {
        var service = CreateService();
        var parameters = new UserGetSelfParams
        {
            UserInformation = CreateUserInformation(Guid.NewGuid())
        };

        var result = await service.GetSelfUser(parameters);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UsersError.UserNotFound));
        _logger.ReceivedLog(LogLevel.Information, "Cannot find user.");
    }

    private UsersService CreateService()
    {
        return new UsersService(_logger, _dbContextFactory);
    }

    private static UserInformationToken CreateUserInformation(Guid userId)
    {
        return new UserInformationToken
        {
            RawToken = "token",
            Username = "testuser",
            UserId = userId
        };
    }
}
