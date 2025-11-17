namespace YourGamesList.Contracts.Requests.Games;

public class SearchYglGamesRequestBody
{
    public required string GameName { get; init; }
    public int Take { get; init; } = 10;
    public int Skip { get; init; } = 0;
}