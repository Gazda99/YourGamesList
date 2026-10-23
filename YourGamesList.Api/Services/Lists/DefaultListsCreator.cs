using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using YourGamesList.Database;
using YourGamesList.Database.Entity;

namespace YourGamesList.Api.Services.Lists;

public interface IDefaultListsCreator
{
    /// <summary>
    /// Adds the default lists for a user.
    /// </summary>
    Task AddDefaultListsToGivenUser(YourGamesListDbContext dbContext, User user);
}

public class DefaultListsCreator : IDefaultListsCreator
{
    private readonly ILogger<DefaultListsCreator> _logger;
    private readonly TimeProvider _timeProvider;

    public DefaultListsCreator(ILogger<DefaultListsCreator> logger, TimeProvider timeProvider)
    {
        _logger = logger;
        _timeProvider = timeProvider;
    }

    public Task AddDefaultListsToGivenUser(YourGamesListDbContext dbContext, User user)
    {
        _logger.LogInformation("Adding default lists...");
        var now = _timeProvider.GetUtcNow();
        const string wishlistName = "Wishlist";
        var wishList = new GamesList()
        {
            CreatedAt = now,
            Name = wishlistName,
            NormalizedName = wishlistName.ToLowerInvariant(),
            User = user,
            Editable = false,
            IsPrivate = true,
            Description = "Games I would like to play."
        };
        
        const string playedName = "Played";
        
        var playedList = new GamesList()
        {
            CreatedAt = now,
            Name = playedName,
            NormalizedName = playedName.ToLowerInvariant(),
            User = user,
            Editable = false,
            IsPrivate = true,
            Description = "Games I have played so far."
        };

        dbContext.GamesLists.Add(wishList);
        dbContext.GamesLists.Add(playedList);
        _logger.LogInformation("Added default lists.");
        return Task.CompletedTask;
    }
}