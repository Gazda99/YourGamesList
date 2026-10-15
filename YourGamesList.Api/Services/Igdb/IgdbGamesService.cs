using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Services.Igdb.Model;
using YourGamesList.Common;

namespace YourGamesList.Api.Services.Igdb;

public interface IIgdbGamesService
{
    /// <summary>
    /// Searches for games by name.
    /// </summary>
    Task<ValueResult<IgdbGame[]>> GetGamesByName(string gameName);

    /// <summary>
    /// Gets games by their IGDB IDs.
    /// </summary>
    Task<ValueResult<IgdbGame[]>> GetGamesByIds(int[] gameIds);
}

public class IgdbGamesService : IIgdbGamesService
{
    private const string RequestGameFields = """
                                             cover.id,
                                             cover.image_id,
                                             cover.image_type.name,
                                             cover.url,
                                             first_release_date,
                                             game_engines.name,
                                             game_type.type,
                                             genres.name,
                                             id,
                                             involved_companies.company.name,
                                             involved_companies.developer,
                                             involved_companies.publisher,
                                             name,
                                             rating_count,
                                             release_dates.date,
                                             release_dates.platform.name,
                                             storyline,
                                             summary,
                                             themes.name;
                                             """;

    private readonly ILogger<IgdbGamesService> _logger;
    private readonly IIgdbService _igdbService;

    public IgdbGamesService(ILogger<IgdbGamesService> logger, IIgdbService igdbService)
    {
        _logger = logger;

        _igdbService = igdbService;
    }


    public async Task<ValueResult<IgdbGame[]>> GetGamesByName(string gameName)
    {
        var query = ApiCalypseQueryBuilder.Build()
            .WithWhere($"name ~ *\"{gameName}\"*")
            .WithFields(RequestGameFields)
            .WithSort("rating_count desc")
            .CreateQuery();

        _logger.LogInformation($"Searching for game '{gameName}'");

        return await _igdbService.CallIgdb<IgdbGame[]>(IgdbEndpoints.Game, query);
    }

    public async Task<ValueResult<IgdbGame[]>> GetGamesByIds(int[] gameIds)
    {
        var ids = string.Join(",", gameIds);
        var query = ApiCalypseQueryBuilder.Build()
            .WithWhere($"id = ({ids})")
            .WithFields(RequestGameFields)
            .CreateQuery();

        _logger.LogInformation($"Searching for '{gameIds.Length}' games with ids: '[{ids}]'");

        return await _igdbService.CallIgdb<IgdbGame[]>(IgdbEndpoints.Game, query);
    }
}