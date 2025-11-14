namespace YourGamesList.Web.Page.Services.Ygl.Model.Requests;

public class SearchGamesRequest
{
    public required string GameName { get; init; }
    public required int Take { get; init; }
    public required int Skip { get; init; }
}