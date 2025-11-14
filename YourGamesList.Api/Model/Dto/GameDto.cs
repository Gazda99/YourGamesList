using System;
using System.Collections.Generic;

namespace YourGamesList.Api.Model.Dto;

public class GameDto
{
    public required Guid Id { get; init; }
    public required long FirstReleaseDate { get; init; }
    public required string GameType { get; init; }
    public required List<string> Genres { get; init; }
    public required long IgdbGameId { get; init; }
    public required string ImageId { get; init; }
    public required string Name { get; init; }
    public required int RatingCount { get; init; }
    public required string StoryLine { get; init; }
    public required string Summary { get; init; }
    public required List<string> Themes { get; init; }
}