namespace YourGamesList.Contracts.Dto;

public class GameDto
{
    public required long IgdbId { get; init; }
    public GameCoverDto? Cover { get; init; }
    public required long FirstReleaseDate { get; init; }
    public required string[] GameEngines { get; init; }
    public required string GameType { get; init; }
    public required string[] Genres { get; init; }
    public required GameCompanyDto[] GameCompanies { get; init; }
    public required string Name { get; init; }
    public required GameReleaseDateDto[] ReleaseDates { get; init; }
    public required string StoryLine { get; init; }
    public required string Summary { get; init; }
    public required string[] Themes { get; init; }
}