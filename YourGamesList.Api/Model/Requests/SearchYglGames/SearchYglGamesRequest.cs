using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace YourGamesList.Api.Model.Requests.SearchYglGames;

public class SearchYglGamesRequest
{
    [FromBody] public required SearchYglGamesRequestBody SearchYglGamesRequestBody { get; init; }
}

public class SearchYglGamesRequestBody
{
    public required string GameName { get; init; }
    public int? Take { get; init; } = 10;
    public int? Skip { get; init; } = 0;
}


//TODO:
internal sealed class SearchYglGamesRequestValidator : AbstractValidator<SearchYglGamesRequest>
{
    public SearchYglGamesRequestValidator()
    {
    }
}