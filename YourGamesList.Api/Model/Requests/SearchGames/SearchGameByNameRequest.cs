using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace YourGamesList.Api.Model.Requests.SearchGames;

public class SearchGameByNameRequest
{
    [FromQuery(Name = "gameName")]
    public required string GameName { get; init; }
}

internal sealed class SearchGameByNameRequestValidator : AbstractValidator<SearchGameByNameRequest>
{
    public SearchGameByNameRequestValidator()
    {
        RuleFor(x => x.GameName)
            .NotEmpty();
    }
}