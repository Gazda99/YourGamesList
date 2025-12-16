using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using YourGamesList.Contracts.Requests.Games;

namespace YourGamesList.Api.Model.Requests.SearchYglGames;

public class SearchYglGamesRequest
{
    [FromBody] public required SearchYglGamesRequestBody Body { get; init; }
}

internal sealed class SearchYglGamesRequestValidator : AbstractValidator<SearchYglGamesRequest>
{
    public SearchYglGamesRequestValidator()
    {
        RuleFor(x => x.Body.GameName)
            .NotEmpty()
            .WithMessage("Game name must be provided.");

        RuleFor(x => x.Body.Take)
            .GreaterThan(0)
            .WithMessage("Take must be greater than 0.");

        RuleFor(x => x.Body.Skip)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Skip must be greater or equal to 0.");
    }
}