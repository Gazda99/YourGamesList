using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using YourGamesList.Api.Model;
using YourGamesList.Api.Services.Lists;
using YourGamesList.Api.Services.Lists.Model;
using YourGamesList.Database;
using YourGamesList.Database.Entity;

namespace YourGamesList.Api.UnitTests.Services.Lists;

public class ListsSearchSqliteTests
{
    [TestCase("łódź")]
    [TestCase("ŁÓDŹ")]
    [TestCase("Łódź")]
    public async Task Search_NonAsciiListAndOwnerName_IsCaseInsensitive(string searchTerm)
    {
        //ARRANGE
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<YourGamesListDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var dbContext = new YourGamesListDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var user = CreateUser("Łódź");
        var list = CreateList(user, "Gry Łódź");
        dbContext.GamesLists.Add(list);
        await dbContext.SaveChangesAsync();

        var service = new ListsManagerService(
            Substitute.For<ILogger<ListsManagerService>>(),
            dbContext,
            new FakeTimeProvider());

        //ACT
        var byListName = await service.Search(new SearchListsParams
        {
            UserInformation = CreateUserInformation(Guid.NewGuid()),
            ListName = searchTerm
        });
        var byOwnerName = await service.Search(new SearchListsParams
        {
            UserInformation = CreateUserInformation(Guid.NewGuid()),
            OwnerName = searchTerm
        });

        //ASSERT
        Assert.That(byListName.Select(x => x.Id), Is.EquivalentTo([list.Id]));
        Assert.That(byOwnerName.Select(x => x.Id), Is.EquivalentTo([list.Id]));
    }

    private static User CreateUser(string username) => new()
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

    private static GamesList CreateList(User user, string name) => new()
    {
        Id = Guid.NewGuid(),
        CreatedAt = DateTimeOffset.UtcNow,
        Editable = true,
        IsPrivate = false,
        Name = name,
        NormalizedName = name.ToLowerInvariant(),
        User = user
    };

    private static UserInformationToken CreateUserInformation(Guid userId) => new()
    {
        RawToken = "token",
        Username = "searcher",
        UserId = userId
    };
}
