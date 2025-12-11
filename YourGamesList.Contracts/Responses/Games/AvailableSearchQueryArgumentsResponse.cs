using System.Collections.Generic;

namespace YourGamesList.Contracts.Responses.Games;

public class AvailableSearchQueryArgumentsResponse
{
    public required List<string> GameTypes { get; init; }
    public required List<string> Genres { get; init; }
    public required List<string> Themes { get; init; }
}