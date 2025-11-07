using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Model.Dto;
using YourGamesList.Api.Services.ModelMapper;
using YourGamesList.Api.Services.Ygl.Games.Model;
using YourGamesList.Database;

namespace YourGamesList.Api.Services.Ygl.Games;

public interface IYglGamesService
{
    Task<List<GameDto>> SearchGames(SearchGamesParameters parameters);
}

//TODO: unit tests
public class YglGamesService : IYglGamesService
{
    private readonly ILogger<YglGamesService> _logger;
    private readonly IYglDatabaseAndDtoMapper _yglDatabaseAndDtoMapper;
    private readonly YglDbContext _yglDbContext;

    public YglGamesService(ILogger<YglGamesService> logger, IDbContextFactory<YglDbContext> yglDbContext, IYglDatabaseAndDtoMapper yglDatabaseAndDtoMapper)
    {
        _logger = logger;
        _yglDatabaseAndDtoMapper = yglDatabaseAndDtoMapper;
        _yglDbContext = yglDbContext.CreateDbContext();
    }

    public async Task<List<GameDto>> SearchGames(SearchGamesParameters parameters)
    {
        //TODO: finish logic
        var gameName = parameters.GameName.ToLower();
        var games = await _yglDbContext.Games.Where(x => x.Name.ToLower().Contains(gameName))
            .Skip(parameters.Skip)
            .Take(parameters.Take)
            .ToListAsync();

        var gameDtos = games
            .Select(game => _yglDatabaseAndDtoMapper.Map(game))
            .OrderBy(x => x.Name.StartsWith(gameName, System.StringComparison.InvariantCultureIgnoreCase))
            .ToList();

        return gameDtos;
    }
}