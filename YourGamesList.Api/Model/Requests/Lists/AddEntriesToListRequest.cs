using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using YourGamesList.Api.Attributes;
using YourGamesList.Contracts.Requests.Lists;

namespace YourGamesList.Api.Model.Requests.Lists;

public class AddEntriesToListRequest
{
    [FromAuthorizeHeader] public required JwtUserInformation UserInformation { get; init; }
    [FromBody] public required AddEntriesToListRequestBody Body { get; init; }
}

internal sealed class AddEntriesToListRequestValidator : AbstractValidator<AddEntriesToListRequest>
{
    public AddEntriesToListRequestValidator(IValidator<JwtUserInformation> jwtUserInformationValidator)
    {
        RuleFor(x => x.UserInformation).SetValidator(jwtUserInformationValidator);

        RuleFor(x => x.Body.ListId)
            .NotEmpty()
            .WithMessage("List Id is required.");

        RuleForEach(x => x.Body.EntriesToAdd)
            .ChildRules(entry =>
            {
                entry.RuleFor(x => x.GameId)
                    .NotEmpty()
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("Game Id is required.");
            });
    }
}