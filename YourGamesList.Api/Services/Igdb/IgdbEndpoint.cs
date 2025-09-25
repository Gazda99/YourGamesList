namespace YourGamesList.Api.Services.Igdb;

public static class IgdbEndpoints
{
    public static readonly IgdbEndpoint Game = new IgdbEndpoint()
    {
        Endpoint = "games"
    };
}

public class IgdbEndpoint
{
    public required string Endpoint { get; init; }
}