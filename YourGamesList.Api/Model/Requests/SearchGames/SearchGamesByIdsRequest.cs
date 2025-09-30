using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace YourGamesList.Api.Model.Requests.SearchGames;

public class SearchGamesByIdsRequest
{
    [FromQuery(Name = "gameIds")] public required int[] GameIds { get; init; }
}

internal sealed class SearchGamesByIdsRequestValidator : AbstractValidator<SearchGamesByIdsRequest>
{
    public SearchGamesByIdsRequestValidator()
    {
        RuleFor(x => x.GameIds)
            .NotEmpty()
            .ForEach(x => { x.GreaterThanOrEqualTo(0); });
    }
}