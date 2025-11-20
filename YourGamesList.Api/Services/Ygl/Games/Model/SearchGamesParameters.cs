using YourGamesList.Contracts.Dto;

namespace YourGamesList.Api.Services.Ygl.Games.Model;

public class SearchGamesParameters
{
    public required string GameName { get; init; }
    public string[]? Themes { get; init; }
    public string[]? Genres { get; init; }
    public string? GameType { get; init; }
    public int? Year { get; set; }
    public TypeOfDateDto? TypeOfDate { get; set; }
    public int Take { get; init; } = 10;
    public int Skip { get; init; } = 0;
}