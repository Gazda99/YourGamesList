namespace YourGamesList.Api.Services.Ygl.Games.Model;

public class SearchGamesParameters
{
    public required string GameName { get; init; }
    public int Take { get; init; } = 10;
    public int Skip { get; init; } = 0;
}