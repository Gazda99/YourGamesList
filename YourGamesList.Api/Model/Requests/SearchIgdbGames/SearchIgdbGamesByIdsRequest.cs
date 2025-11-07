using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace YourGamesList.Api.Model.Requests.SearchIgdbGames;

public class SearchIgdbGamesByIdsRequest
{
    [FromQuery(Name = "gameIds")] public required int[] GameIds { get; init; }
}

internal sealed class SearchGamesByIdsRequestValidator : AbstractValidator<SearchIgdbGamesByIdsRequest>
{
    public SearchGamesByIdsRequestValidator()
    {
        RuleFor(x => x.GameIds)
            .NotEmpty()
            .ForEach(x => { x.GreaterThanOrEqualTo(0); });
    }
}