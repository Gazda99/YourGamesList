using System;
using System.Linq;
using YourGamesList.Api.Model.Dto;
using YourGamesList.Database.Entities;

namespace YourGamesList.Api.Services.ModelMappers;

public interface IYglDatabaseAndDtoMapper
{
    GamesListDto Map(GamesList gamesList);
    GameListEntryDto Map(GameListEntry gameListEntry);
    GameDto Map(Game game);
    UserDto Map(User user);
    CompletionStatusDto Map(CompletionStatus completionStatus);
    GameDistributionDto Map(GameDistribution gameDistribution);
    PlatformDto Map(Platform platform);


    CompletionStatus Map(CompletionStatusDto completionStatus);
    GameDistribution Map(GameDistributionDto gameDistribution);
    Platform Map(PlatformDto platform);
}

public class YglDatabaseAndDtoMapper : IYglDatabaseAndDtoMapper
{
    public GamesListDto Map(GamesList gamesList)
    {
        return new GamesListDto()
        {
            Id = gamesList.Id,
            Desc = gamesList.Desc,
            Name = gamesList.Name,
            IsPublic = gamesList.IsPublic,
            CanBeDeleted = gamesList.CanBeDeleted,
            Entries = gamesList.Entries.Select(Map).ToList(),
        };
    }


    public GameListEntryDto Map(GameListEntry gameListEntry)
    {
        return new GameListEntryDto()
        {
            Id = gameListEntry.Id,
            Game = gameListEntry.Game != null ? Map(gameListEntry.Game) : null,
            Desc = gameListEntry.Desc,
            Platforms = gameListEntry.Platforms.Select(Map).ToArray(),
            GameDistributions = gameListEntry.GameDistributions.Select(Map).ToArray(),
            IsStarred = gameListEntry.IsStarred,
            Rating = gameListEntry.Rating,
            CompletionStatus = Map(gameListEntry.CompletionStatus)
        };
    }


    public GameDto Map(Game game)
    {
        return new GameDto()
        {
            Id = game.Id,
            FirstReleaseDate = game.FirstReleaseDate,
            GameType = game.GameType,
            Genres = game.Genres.ToList(),
            IgdbGameId = game.IgdbGameId,
            ImageId = game.ImageId,
            Name = game.Name,
            StoryLine = game.StoryLine,
            Summary = game.Summary,
            Themes = game.Themes.ToList(),
            RatingCount = game.RatingCount
        };
    }

    public UserDto Map(User user)
    {
        return new UserDto()
        {
            Id = user.Id,
            Username = user.Username,
            DateOfBirth = user.DateOfBirth,
            Description = user.Description,
            Country = user.Country
        };
    }
    
    public CompletionStatusDto Map(CompletionStatus completionStatus)
    {
        return MapEnums<CompletionStatus, CompletionStatusDto>(completionStatus);
    }

    public GameDistributionDto Map(GameDistribution gameDistribution)
    {
        return MapEnums<GameDistribution, GameDistributionDto>(gameDistribution);
    }

    public PlatformDto Map(Platform platform)
    {
        return MapEnums<Platform, PlatformDto>(platform);
    }

    public CompletionStatus Map(CompletionStatusDto completionStatus)
    {
        return MapEnums<CompletionStatusDto, CompletionStatus>(completionStatus);
    }

    public GameDistribution Map(GameDistributionDto gameDistribution)
    {
        return MapEnums<GameDistributionDto, GameDistribution>(gameDistribution);
    }

    public Platform Map(PlatformDto platform)
    {
        return MapEnums<PlatformDto, Platform>(platform);
    }

    private static TDestination MapEnums<TSource, TDestination>(TSource source)
        where TSource : Enum
        where TDestination : Enum
    {
        var sourceName = source.ToString();
        try
        {
            return (TDestination) Enum.Parse(typeof(TDestination), sourceName);
        }
        catch (ArgumentException ex)
        {
            throw new InvalidOperationException($"Could not map '{sourceName}' from '{typeof(TSource).Name}' yo '{typeof(TDestination).Name}'.", ex);
        }
    }
}