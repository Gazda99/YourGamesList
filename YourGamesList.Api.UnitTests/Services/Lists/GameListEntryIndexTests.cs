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

    [Test]
    public async Task UniqueNormalizedListName_IsScopedToEachUser()
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
        var otherUser = CreateUser();
        dbContext.GamesLists.AddRange(
            CreateList(user, "Collection"),
            CreateList(otherUser, "collection"));
        await dbContext.SaveChangesAsync();

        //ACT
        dbContext.GamesLists.Add(CreateList(user, "COLLECTION"));

        //ASSERT
        await Assert.ThrowsAsync<DbUpdateException>(async () => await dbContext.SaveChangesAsync());
    }

    [Test]
    public async Task UniqueNormalizedUsername_RejectsUsernameDifferingOnlyByCase()
    {
        //ARRANGE
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<YourGamesListDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var dbContext = new YourGamesListDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        dbContext.Users.Add(CreateUser("Bob"));
        await dbContext.SaveChangesAsync();

        //ACT
        dbContext.Users.Add(CreateUser("bob"));

        //ASSERT
        await Assert.ThrowsAsync<DbUpdateException>(async () => await dbContext.SaveChangesAsync());
    }

    private static User CreateUser(string? username = null)
    {
        username ??= Guid.NewGuid().ToString("N");
        return new User
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            DateOfBirth = new DateOnly(2000, 1, 1),
            Email = $"{Guid.NewGuid():N}@example.com",
            PasswordHash = "hashed-password",
            UserStatus = UserStatus.Active,
            Username = username,
            NormalizedUsername = username.ToLowerInvariant()
        };
    }

    private static GamesList CreateList(User user, string? name = null)
    {
        name ??= Guid.NewGuid().ToString("N");
        return new GamesList
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            Editable = true,
            IsPrivate = true,
            Name = name,
            NormalizedName = name.ToLowerInvariant(),
            User = user
        };
    }

    private static GamesListEntry CreateEntry(Guid gamesListId, long igdbId) => new()
    {
        Id = Guid.NewGuid(),
        CreatedAt = DateTimeOffset.UtcNow,
        CompletionStatus = CompletionStatus.Played,
        GamesListId = gamesListId,
        IgdbId = igdbId
    };
}
