using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YourGamesList.Api.Services.ModelMappers;
using YourGamesList.Api.Services.Ygl.Games.Model;
using YourGamesList.Contracts.Dto;
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
        var gameName = parameters.GameName.ToLower();
        var gamesQuery = _yglDbContext.Games
            .AsQueryable()
            .AsNoTracking()
            .Where(x => x.Name.ToLower().Contains(gameName.ToLower()));


        if (parameters.Themes != null && parameters.Themes.Length != 0)
        {
            gamesQuery = gamesQuery.Where(x => parameters.Themes.All(theme => x.Themes.Select(xx => xx.ToLower()).Contains(theme.ToLower())));
        }

        if (parameters.Genres != null && parameters.Genres.Length != 0)
        {
            gamesQuery = gamesQuery.Where(x => parameters.Genres.All(genre => x.Genres.Select(xx => xx.ToLower()).Contains(genre.ToLower())));
        }

        if (!string.IsNullOrWhiteSpace(parameters.GameType))
        {
            gamesQuery = gamesQuery.Where(x => x.GameType.ToLower().Contains(parameters.GameType.ToLower()));
        }

        if (parameters.Year != null && parameters.TypeOfDate != null)
        {
            var startOfTargetYear = GetStartOfYearTimestamp(parameters.Year.Value);
            var startOfNextYear = GetStartOfYearTimestamp(parameters.Year.Value + 1);

            switch (parameters.TypeOfDate)
            {
                case TypeOfDateDto.Exact:

                    gamesQuery = gamesQuery.Where(x => x.FirstReleaseDate >= startOfTargetYear && x.FirstReleaseDate <= startOfNextYear);
                    break;
                case TypeOfDateDto.Before:
                    gamesQuery = gamesQuery.Where(x => x.FirstReleaseDate <= startOfNextYear);
                    break;
                case TypeOfDateDto.After:
                    gamesQuery = gamesQuery.Where(x => x.FirstReleaseDate >= startOfTargetYear);
                    break;
            }
        }

        var games = await gamesQuery
            .OrderByDescending(x => x.RatingCount)
            .Skip(parameters.Skip)
            .Take(parameters.Take)
            .ToListAsync();

        var gameDtos = games
            .Select(game => _yglDatabaseAndDtoMapper.Map(game))
            .ToList();

        return gameDtos;
    }

    private static long GetStartOfYearTimestamp(int year)
    {
        var startOfYear = new DateTimeOffset(year, 1, 1, 0, 0, 0, TimeSpan.Zero);
        return startOfYear.ToUnixTimeSeconds();
    }
}