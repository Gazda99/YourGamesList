using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using YourGamesList.Api.Services.Lists;
using YourGamesList.Database;
using YourGamesList.Database.Entity;
using YourGamesList.TestsUtils;

namespace YourGamesList.Api.UnitTests.Services.Lists;

public class DefaultListsCreatorTests
{
    private ILogger<DefaultListsCreator> _logger;
    private FakeTimeProvider _timeProvider;
    private YourGamesListDbContext _dbContext;

    [SetUp]
    public void SetUp()
    {
        _logger = Substitute.For<ILogger<DefaultListsCreator>>();
        _timeProvider = new FakeTimeProvider();

        var options = new DbContextOptionsBuilder<YourGamesListDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new YourGamesListDbContext(options);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Test]
    public async Task AddDefaultListsToGivenUser_AddsWishlistAndPlayedListsForUser()
    {
        var now = DateTimeOffset.UtcNow;
        _timeProvider.SetUtcNow(now);

        var user = new User
        {
            Id = Guid.NewGuid(),
            CreatedAt = now,
            DateOfBirth = new DateOnly(2000, 1, 1),
            Email = "user@example.com",
            PasswordHash = "hashed_password",
            UserStatus = UserStatus.Active,
            Username = "testuser"
        };
        await _dbContext.Users.AddAsync(user);

        var creator = new DefaultListsCreator(_logger, _timeProvider);

        await creator.AddDefaultListsToGivenUser(_dbContext, user);
        await _dbContext.SaveChangesAsync();

        var lists = await _dbContext.GamesLists
            .Where(list => list.UserId == user.Id)
            .ToListAsync();

        Assert.That(lists, Has.Count.EqualTo(2));
        var playedList = lists.FirstOrDefault(list => list.Name == "Played");
        Assert.That(playedList, Is.Not.Null);
        Assert.That(playedList.Description, Is.EqualTo("Games I have played so far."));
        var wishList = lists.FirstOrDefault(list => list.Name == "Wishlist");
        Assert.That(wishList, Is.Not.Null);
        Assert.That(wishList.Description, Is.EqualTo("Games I would like to play."));

        Assert.That(lists, Has.All.Matches<GamesList>(list =>
            list.CreatedAt == now &&
            !list.Editable &&
            list.IsPrivate &&
            list.UserId == user.Id));

        _logger.ReceivedLog(LogLevel.Information, "Adding default lists...");
        _logger.ReceivedLog(LogLevel.Information, "Added default lists.");
    }
}