using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using YourGamesList.Api.Attributes;
using YourGamesList.Contracts.Requests.Lists;

namespace YourGamesList.Api.Model.Requests.Lists;

public class AddOwnershipInfoToEntryRequest
{
    [FromAuthorizeHeader] public required UserInformationToken UserInformation { get; init; }
    [FromBody] public required AddOwnershipInfoToEntryRequestBody Body { get; init; }
}

internal sealed class AddOwnershipInfoToEntryRequestValidator : AbstractValidator<AddOwnershipInfoToEntryRequest>
{
    public AddOwnershipInfoToEntryRequestValidator(IValidator<UserInformationToken> jwtUserInformationValidator)
    {
        RuleFor(x => x.UserInformation).SetValidator(jwtUserInformationValidator);

        RuleFor(x => x.Body.ListEntryId)
            .NotEmpty()
            .WithMessage("List entry Id is required.");
    }
}