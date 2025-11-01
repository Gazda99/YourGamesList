using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Services.Igdb.Model;
using YourGamesList.Common;

namespace YourGamesList.Api.Services.Igdb;

public interface IGamesIgdbService
{
    Task<ValueResult<IgdbGame[]>> GetGamesByName(string gameName);
    Task<ValueResult<IgdbGame[]>> GetGamesByIds(int[] gameIds);
}

//TODO: unit tests
public class GamesIgdbService : IGamesIgdbService
{
    private const string RequestGameFields = "cover.*,first_release_date,game_type.type,genres.name,id,name,rating_count,storyline,summary,themes.name";

    private readonly ILogger<GamesIgdbService> _logger;
    private readonly IIgdbService _igdbService;

    public GamesIgdbService(ILogger<GamesIgdbService> logger, IIgdbService igdbService)
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

        _logger.LogInformation($"Searching for '{gameIds.Length}' games with ids: '{ids}'");

        return await _igdbService.CallIgdb<IgdbGame[]>(IgdbEndpoints.Game, query);
    }
}