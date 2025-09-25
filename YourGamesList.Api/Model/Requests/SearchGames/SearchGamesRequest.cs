using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace YourGamesList.Api.Model.Requests.SearchGames;

public class SearchGamesRequest
{
    [FromQuery(Name = "gameName")]
    public required string GameName { get; init; }
}

internal sealed class SearchGamesRequestValidator : AbstractValidator<SearchGamesRequest>
{
    public SearchGamesRequestValidator()
    {
        RuleFor(x => x.GameName)
            .NotEmpty();
    }
}