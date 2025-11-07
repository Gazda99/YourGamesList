using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace YourGamesList.Api.Model.Requests.SearchIgdbGames;

public class SearchIgdbGameByNameRequest
{
    [FromQuery(Name = "gameName")]
    public required string GameName { get; init; }
}

internal sealed class SearchGameByNameRequestValidator : AbstractValidator<SearchIgdbGameByNameRequest>
{
    public SearchGameByNameRequestValidator()
    {
        RuleFor(x => x.GameName)
            .NotEmpty();
    }
}