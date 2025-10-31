namespace YourGamesList.Api.Services.Igdb;

public static class IgdbEndpoints
{
    public static readonly IgdbEndpoint Game = new IgdbEndpoint()
    {
        Endpoint = "games"
    };

    public static readonly IgdbEndpoint MultiQuery = new IgdbEndpoint()
    {
        Endpoint = "multiquery"
    };
}

public class IgdbEndpoint
{
    public required string Endpoint { get; init; }
}