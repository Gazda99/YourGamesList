using AutoFixture;
using Microsoft.Extensions.Options;
using NSubstitute;
using YourGamesList.Database.Entities;
using YourGamesList.Database.Options;

namespace YourGamesList.Database.TestUtils;

/// <summary>
/// YglDbContext builder for unit tests. Allows seeding of DbSets with test data. Uses an in-memory SQLite database.
/// </summary>
public class TestYglDbContextBuilder
{
    private readonly YglDbContext _yglDbContext;

    private TestYglDbContextBuilder(IOptions<YourGamesListDatabaseOptions>? iOptions = null)
    {
        if (iOptions == null)
        {
            iOptions = Substitute.For<IOptions<YourGamesListDatabaseOptions>>();
            var options = new Fixture()
                .Build<YourGamesListDatabaseOptions>()
                .With(x => x.ConnectionString, "DataSource=file::memory:?cache=shared")
                .With(x => x.MigrationAssembly, (string?) null)
                .Create();
            iOptions.Value.Returns(options);
        }

        _yglDbContext = new YglDbContext(iOptions);
        _yglDbContext.Database.EnsureCreated();
    }

    public static TestYglDbContextBuilder Build(IOptions<YourGamesListDatabaseOptions>? iOptions = null)
    {
        return new TestYglDbContextBuilder(iOptions);
    }

    /// <summary>
    /// Seeds the Users DbSet with the provided list of users.
    /// </summary>
    public TestYglDbContextBuilder WithUserDbSet(List<User> seedUsers)
    {
        _yglDbContext.Users.AddRange(seedUsers);
        _yglDbContext.SaveChanges();
        _yglDbContext.ChangeTracker.Clear();
        return this;
    }

    /// <summary>
    /// Seeds the Games DbSet with the provided list of games.
    /// </summary>
    public TestYglDbContextBuilder WithGamesDbSet(List<Game> seedGames)
    {
        _yglDbContext.Games.AddRange(seedGames);
        _yglDbContext.SaveChanges();
        _yglDbContext.ChangeTracker.Clear();
        return this;
    }

    /// <summary>
    /// Seeds the GameListEntries DbSet with the provided list of game list entries.
    /// </summary>
    public TestYglDbContextBuilder WithGameListEntriesDbSet(List<GameListEntry> seedGameListEntries)
    {
        _yglDbContext.GameListEntries.AddRange(seedGameListEntries);
        _yglDbContext.SaveChanges();
        _yglDbContext.ChangeTracker.Clear();
        return this;
    }

    /// <summary>
    /// Seeds the Lists DbSet with the provided list of games lists.
    /// </summary>
    public TestYglDbContextBuilder WithListsDbSet(List<GamesList> seedGamesLists)
    {
        _yglDbContext.Lists.AddRange(seedGamesLists);
        _yglDbContext.SaveChanges();
        _yglDbContext.ChangeTracker.Clear();
        return this;
    }

    /// <summary>
    /// Returns the underlying YglDbContext instance.
    /// </summary>
    public YglDbContext Get()
    {
        return _yglDbContext;
    }
}