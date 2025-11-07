using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using YourGamesList.Api.Attributes;

namespace YourGamesList.Api.Model.Requests.Lists;

public class SearchListsRequest
{
    [FromAuthorizeHeader] public required JwtUserInformation UserInformation { get; init; }
    [FromBody] public required SearchListsRequestBody Body { get; init; }
}

public class SearchListsRequestBody
{
    public string? ListName { get; set; }
    public string? UserName { get; init; }
    public bool IncludeGames { get; init; } = false;
    public int Take { get; init; } = 10;
    public int Skip { get; init; } = 0;
}

internal sealed class SearchListsRequestValidator : AbstractValidator<SearchListsRequest>
{
    public SearchListsRequestValidator(IValidator<JwtUserInformation> jwtUserInformationValidator)
    {
        RuleFor(x => x.UserInformation).SetValidator(jwtUserInformationValidator);

        RuleFor(x => new { x.Body.UserName, x.Body.ListName })
            .Must(x => !string.IsNullOrWhiteSpace(x.UserName) && !string.IsNullOrWhiteSpace(x.ListName))
            .WithMessage("You must provide user name or list name.");

        //If List Name is provided, validate its length
        When(x => !string.IsNullOrEmpty(x.Body.ListName), () =>
        {
            RuleFor(x => x.Body.ListName)
                .Must(x => x?.Length >= 3)
                .WithMessage("List name must be at least 3 characters long.");
        });

        RuleFor(x => x.Body.Take)
            .GreaterThan(0)
            .WithMessage("Take must be greater than 0.");

        RuleFor(x => x.Body.Skip)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Skip must be greater or equal to 0.");
    }
}