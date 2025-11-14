using System.Collections.Generic;

namespace YourGamesList.Api.Services.Ygl.Games.Model;

public class AvailableSearchQueryArguments
{
    public required List<string> GameTypes { get; init; }
    public required List<string> Genres { get; init; }
    public required List<string> Themes { get; init; }
}