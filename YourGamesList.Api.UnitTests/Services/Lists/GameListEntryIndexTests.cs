using System;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using YourGamesList.Database;
using YourGamesList.Database.Entity;

namespace YourGamesList.Api.UnitTests.Services.Lists;

public class GameListEntryIndexTests
{
    [Test]
    public async Task UniqueIgdbId_IsScopedToEachList()
    {
        //ARRANGE
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<YourGamesListDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var dbContext = new YourGamesListDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var user = CreateUser();
        var firstList = CreateList(user);
        var secondList = CreateList(user);
        dbContext.GamesLists.AddRange(firstList, secondList);
        await dbContext.SaveChangesAsync();
        dbContext.GameListEntries.AddRange(
            CreateEntry(firstList.Id, 123),
            CreateEntry(secondList.Id, 123));
        await dbContext.SaveChangesAsync();

        //ACT
        dbContext.GameListEntries.Add(CreateEntry(firstList.Id, 123));

        //ASSERT
        await Assert.ThrowsAsync<DbUpdateException>(async () => await dbContext.SaveChangesAsync());
    }

    [TestCase((byte)0)]
    [TestCase((byte)5)]
    public async Task RatingCheckConstraint_AllowsRatingWithinRange(byte rating)
    {
        //ARRANGE
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<YourGamesListDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var dbContext = new YourGamesListDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var list = CreateList(CreateUser());
        dbContext.GamesLists.Add(list);
        await dbContext.SaveChangesAsync();

        var entry = CreateEntry(list.Id, 123);
        entry.Rating = rating;

        //ACT
        dbContext.GameListEntries.Add(entry);

        //ASSERT
        await Assert.DoesNotThrowAsync(async () => await dbContext.SaveChangesAsync());
    }

    [Test]
    public async Task RatingCheckConstraint_RejectsRatingAboveMaximum()
    {
        //ARRANGE
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<YourGamesListDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var dbContext = new YourGamesListDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var list = CreateList(CreateUser());
        dbContext.GamesLists.Add(list);
        await dbContext.SaveChangesAsync();

        var entry = CreateEntry(list.Id, 123);
        entry.Rating = 1;

        //ACT
        dbContext.GameListEntries.Add(entry);

        //ASSERT
        await Assert.ThrowsAsync<DbUpdateException>(async () => await dbContext.SaveChangesAsync());
    }

    private static User CreateUser() => new()
    {
        Id = Guid.NewGuid(),
        CreatedAt = DateTimeOffset.UtcNow,
        DateOfBirth = new DateOnly(2000, 1, 1),
        Email = $"{Guid.NewGuid():N}@example.com",
        PasswordHash = "hashed-password",
        UserStatus = UserStatus.Active,
        Username = Guid.NewGuid().ToString("N")
    };

    private static GamesList CreateList(User user) => new()
    {
        Id = Guid.NewGuid(),
        CreatedAt = DateTimeOffset.UtcNow,
        Editable = true,
        IsPrivate = true,
        Name = Guid.NewGuid().ToString("N"),
        User = user
    };

    private static GamesListEntry CreateEntry(Guid gamesListId, long igdbId) => new()
    {
        Id = Guid.NewGuid(),
        CreatedAt = DateTimeOffset.UtcNow,
        CompletionStatus = CompletionStatus.Played,
        GamesListId = gamesListId,
        IgdbId = igdbId
    };
}
