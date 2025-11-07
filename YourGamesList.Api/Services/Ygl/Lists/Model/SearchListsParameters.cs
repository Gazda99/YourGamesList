namespace YourGamesList.Api.Services.Ygl.Lists.Model;

public class SearchListsParameters
{
    public string? ListName { get; init; } 
    public string? UserName { get; init; } 
    public bool IncludeGames { get; init; } = false;
    public int Take { get; init; } = 10;
    public int Skip { get; init; } = 0;
}