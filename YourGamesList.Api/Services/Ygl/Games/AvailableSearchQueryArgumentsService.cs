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
        var uniqueGameTypesTask = _yglDbContext.Games
            .Where(g => !string.IsNullOrWhiteSpace(g.GameType))
            .Select(g => g.GameType)
            .Distinct()
            .ToListAsync();
        var allGenresTask = _yglDbContext.Games
            .Where(g => g.Genres.Any())
            .Select(g => g.Genres)
            .ToListAsync();
        var allThemesTask = _yglDbContext.Games
            .Where(g => g.Themes.Any())
            .Select(g => g.Themes)
            .ToListAsync();

        await Task.WhenAll(uniqueGameTypesTask, allGenresTask, allThemesTask);

        var uniqueGameTypes = uniqueGameTypesTask.Result;

        var uniqueGenres = allGenresTask.Result
            .SelectMany(g => g)
            .Distinct()
            .ToList();

        var uniqueThemes = allThemesTask.Result
            .SelectMany(t => t)
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