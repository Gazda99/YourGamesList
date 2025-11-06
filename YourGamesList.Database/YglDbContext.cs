using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using YourGamesList.Database.Entities;
using YourGamesList.Database.Options;

namespace YourGamesList.Database;

public class YglDbContext : DbContext
{
    private readonly IOptions<YourGamesListDatabaseOptions> _options;

    public DbSet<User> Users { get; set; }
    public DbSet<Game> Games { get; set; }
    public DbSet<GameListEntry> GameListEntries { get; set; }
    public DbSet<GamesList> Lists { get; set; }

    public YglDbContext(IOptions<YourGamesListDatabaseOptions> options)
    {
        _options = options;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(
            _options.Value.ConnectionString,
            ob =>
            {
                if (!string.IsNullOrWhiteSpace(_options.Value.MigrationAssembly))
                {
                    ob.MigrationsAssembly(_options.Value.MigrationAssembly);
                }
            });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasMany(x => x.GamesLists)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Game>().HasKey(x => x.Id);

        modelBuilder.Entity<GameListEntry>().HasKey(x => x.Id);

        modelBuilder.Entity<GamesList>().HasKey(x => x.Id);
        modelBuilder.Entity<GamesList>()
            .HasMany(x => x.Games)
            .WithOne(x => x.GamesList)
            .HasForeignKey(x => x.GamesListId)
            .HasForeignKey(x => x.GameId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}