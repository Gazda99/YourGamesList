using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using YourGamesList.Database.Entity;
using YourGamesList.Database.Options;

namespace YourGamesList.Database;

[ExcludeFromCodeCoverage]
public class YourGamesListDbContext : DbContext
{
    private readonly IOptions<YourGamesListDatabaseOptions> _options;

    public DbSet<User> Users { get; set; }
    public DbSet<GamesList> GamesLists { get; set; }
    public DbSet<GamesListEntry> GameListEntries { get; set; }

    public YourGamesListDbContext(IOptions<YourGamesListDatabaseOptions> options)
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
                })
            .UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasMany(x => x.GamesLists)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => x.Username).IsUnique();
        });

        modelBuilder.Entity<GamesList>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasMany(x => x.GameListEntries)
                .WithOne(x => x.GamesList)
                .HasForeignKey(x => x.GamesListId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GamesListEntry>(entity => { entity.HasKey(x => x.Id); });

        base.OnModelCreating(modelBuilder);
    }
}