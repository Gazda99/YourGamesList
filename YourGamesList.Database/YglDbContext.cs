using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using YourGamesList.Database.Entities;
using YourGamesList.Database.Options;

namespace YourGamesList.Database;

[ExcludeFromCodeCoverage]
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
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasMany(x => x.GamesLists)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => x.Username);
        });

        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasKey(x => x.Id);
            // entity.HasIndex(x => x.IgdbGameId);
        });

        modelBuilder.Entity<GameListEntry>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasOne(x => x.Game)
                .WithMany(x => x.GameListEntries)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GamesList>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasMany(x => x.Entries)
                .WithOne(x => x.GamesList)
                .HasForeignKey(x => x.GamesListId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}