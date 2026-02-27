using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using YourGamesList.Api.Attributes;
using YourGamesList.Contracts.Requests.Lists;

namespace YourGamesList.Api.Model.Requests.Lists;

public class DeleteOwnershipInfoToEntryRequest
{
    [FromAuthorizeHeader] public required JwtUserInformation UserInformation { get; init; }
    [FromBody] public required DeleteOwnershipInfoToEntryRequestBody Body { get; init; }
}

internal sealed class DeleteOwnershipInfoToEntryRequestValidator : AbstractValidator<DeleteOwnershipInfoToEntryRequest>
{
    public DeleteOwnershipInfoToEntryRequestValidator(IValidator<JwtUserInformation> jwtUserInformationValidator)
    {
        RuleFor(x => x.UserInformation).SetValidator(jwtUserInformationValidator);
        RuleFor(x => x.Body)
            .NotEmpty()
            .WithMessage("Request body is empty.");

        RuleFor(x => x.Body!.ListEntryId)
            .NotEmpty()
            .WithMessage("List entry id is required.");
    }
}