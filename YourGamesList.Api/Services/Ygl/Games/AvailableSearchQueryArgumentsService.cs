using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Services.Ygl.Games.Model;
using YourGamesList.Database;

namespace YourGamesList.Api.Services.Ygl.Games;

public interface IAvailableSearchQueryArgumentsService
{
    Task<AvailableSearchQueryArguments> GetAvailableSearchParams();
}

//TODO: unit tests
public class AvailableSearchQueryArgumentsService : IAvailableSearchQueryArgumentsService
{
    private readonly ILogger<AvailableSearchQueryArgumentsService> _logger;
    private readonly YglDbContext _yglDbContext;

    public AvailableSearchQueryArgumentsService(ILogger<AvailableSearchQueryArgumentsService> logger, IDbContextFactory<YglDbContext> yglDbContext)
    {
        _logger = logger;
        _yglDbContext = yglDbContext.CreateDbContext();
    }

    //This is heavy operation, should be cached on higher level if needed frequently
    public async Task<AvailableSearchQueryArguments> GetAvailableSearchParams()
    {
        _logger.LogInformation("Getting available search query arguments for YGL games.");
        var games = await _yglDbContext.Games
            .AsNoTracking()
            .Select(x => new
            {
                x.GameType,
                x.Genres,
                x.Themes
            })
            .ToListAsync();

        var uniqueGameTypes = games
            .Select(g => g.GameType)
            .Distinct()
            .ToList();

        var uniqueGenres = games
            .SelectMany(g => g.Genres)
            .Distinct()
            .ToList();

        var uniqueThemes = games
            .SelectMany(g => g.Themes)
            .Distinct()
            .ToList();

        _logger.LogInformation("Successfully retrieved available search query arguments for YGL games.");
        return new AvailableSearchQueryArguments()
        {
            GameTypes = uniqueGameTypes,
            Genres = uniqueGenres,
            Themes = uniqueThemes
        };
    }
}