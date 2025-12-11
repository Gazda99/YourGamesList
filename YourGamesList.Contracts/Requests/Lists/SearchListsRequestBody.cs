namespace YourGamesList.Contracts.Requests.Lists;

public class SearchListsRequestBody
{
    public string? ListName { get; set; }
    public string? UserName { get; init; }
    public bool IncludeGames { get; init; } = false;
    public int Take { get; init; } = 10;
    public int Skip { get; init; } = 0;
}