using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using YourGamesList.Api.Attributes;
using YourGamesList.Contracts.Requests.Lists;

namespace YourGamesList.Api.Model.Requests.Lists;

public class DeleteListEntriesRequest
{
    [FromAuthorizeHeader] public required JwtUserInformation UserInformation { get; init; }

    [FromBody] public required DeleteListEntriesRequestBody Body { get; init; }
}

internal sealed class DeleteListEntriesRequestValidator : AbstractValidator<DeleteListEntriesRequest>
{
    public DeleteListEntriesRequestValidator(IValidator<JwtUserInformation> jwtUserInformationValidator)
    {
        RuleFor(x => x.UserInformation).SetValidator(jwtUserInformationValidator);
        RuleFor(x => x.Body)
            .NotEmpty()
            .WithMessage("Request body is empty.");

        RuleFor(x => x.Body!.ListId)
            .NotEmpty()
            .WithMessage("List id is required.");
    }
}