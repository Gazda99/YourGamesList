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

//TODO: unit tests
internal sealed class DeleteEntriesFromListRequestValidator : AbstractValidator<DeleteEntriesFromListRequest>
{
    public DeleteEntriesFromListRequestValidator()
    {
        RuleFor(x => x.UserInformation).SetValidator(new JwtUserInformationValidator());
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