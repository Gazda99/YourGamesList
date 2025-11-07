using System;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using YourGamesList.Api.Attributes;

namespace YourGamesList.Api.Model.Requests.Lists;

public class DeleteEntriesFromListRequest
{
    [FromAuthorizeHeader] public required JwtUserInformation UserInformation { get; init; }

    [FromBody] public DeleteEntriesFromListRequestBody? Body { get; init; }
}

public class DeleteEntriesFromListRequestBody
{
    public required Guid ListId { get; init; }
    public Guid[] EntriesToRemove { get; init; } = [];
}

internal sealed class DeleteEntriesFromListRequestValidator : AbstractValidator<DeleteEntriesFromListRequest>
{
    public DeleteEntriesFromListRequestValidator(IValidator<JwtUserInformation> jwtUserInformationValidator)
    {
        RuleFor(x => x.UserInformation).SetValidator(jwtUserInformationValidator);
        RuleFor(x => x.Body)
            .NotEmpty()
            .WithMessage("Request body is empty");
        When(x => x.Body != null, () =>
        {
            RuleFor(x => x.Body!.ListId)
                .NotEmpty()
                .WithMessage("List id is required");
        });
    }
}