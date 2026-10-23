using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using YourGamesList.Database.Entity;
using YourGamesList.Database.Options;

namespace YourGamesList.Database;

[ExcludeFromCodeCoverage]
public class YourGamesListDbContext : DbContext
{
    private readonly IOptions<YourGamesListDatabaseOptions>? _yglOptions;

    public DbSet<User> Users { get; set; }
    public DbSet<GamesList> GamesLists { get; set; }
    public DbSet<GamesListEntry> GameListEntries { get; set; }
    public DbSet<OwnershipInfo> OwnershipInfos { get; set; }
    public DbSet<EmailVerification> EmailVerifications { get; set; }
    
    public YourGamesListDbContext(DbContextOptions<YourGamesListDbContext> options, IOptions<YourGamesListDatabaseOptions>? yglOptions = null) : base(options)
    {
        _yglOptions = yglOptions;
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured && _yglOptions?.Value != null)
        {
            optionsBuilder.UseSqlite(
                    _yglOptions.Value.ConnectionString,
                    ob =>
                    {
                        if (!string.IsNullOrWhiteSpace(_yglOptions.Value.MigrationAssembly))
                        {
                            ob.MigrationsAssembly(_yglOptions.Value.MigrationAssembly);
                        }
                    })
                .UseSnakeCaseNamingConvention();
        }
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

            entity.HasIndex(x => x.NormalizedUsername).IsUnique();
            entity.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<GamesList>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasMany(x => x.GamesListEntries)
                .WithOne(x => x.GamesList)
                .HasForeignKey(x => x.GamesListId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.UserId, x.NormalizedName })
                .IsUnique();
        });

        modelBuilder.Entity<GamesListEntry>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.ToTable(table => table.HasCheckConstraint(
                "CK_GamesListEntry_Rating",
                $"\"{nameof(GamesListEntry.Rating)}\" IS NULL OR \"{nameof(GamesListEntry.Rating)}\" BETWEEN 0 AND 5"));

            entity.HasIndex(x => new { x.GamesListId, x.IgdbId })
                .IsUnique();

            entity.HasMany(x => x.OwnershipInfo)
                .WithOne(x => x.GameListEntry)
                .HasForeignKey(x => x.GameListEntryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OwnershipInfo>(entity => { entity.HasKey(x => x.Id); });

        modelBuilder.Entity<EmailVerification>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.Token).IsUnique();
            entity.HasOne(x => x.User)
                .WithOne()
                .HasForeignKey<EmailVerification>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        base.OnModelCreating(modelBuilder);
    }
}