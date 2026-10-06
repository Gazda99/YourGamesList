namespace YourGamesList.Contracts.Dto;

public class GameDto
{
    public required long IgdbId { get; set; }
    public GameCoverDto? Cover { get; set; }
    public required long FirstReleaseDate { get; set; }
    public required string[] GameEngines { get; set; }
    public required string GameType { get; set; }
    public required string[] Genres { get; set; }
    public required GameCompanyDto[] GameCompanies { get; set; }
    public required string Name { get; set; }
    public required GameReleaseDateDto[] ReleaseDates { get; set; }
    public required string StoryLine { get; set; }
    public required string Summary { get; set; }
    public required string[] Themes { get; set; }
}