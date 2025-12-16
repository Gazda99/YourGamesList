using YourGamesList.Contracts.Dto;

namespace YourGamesList.Contracts.Requests.Games;

public class SearchYglGamesRequestBody
{
    public required string GameName { get; init; }
    public string[]? Themes { get; set; }
    public string[]? Genres { get; set; }
    public string? GameType { get; set; }
    public ReleaseYearQuery? ReleaseYearQuery { get; set; }

    public int Take { get; init; } = 10;
    public int Skip { get; init; } = 0;
}

public class ReleaseYearQuery
{
    public required int Year { get; set; }
    public required TypeOfDateDto TypeOfDate { get; set; }
}