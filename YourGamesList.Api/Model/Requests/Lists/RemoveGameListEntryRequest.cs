using System;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using YourGamesList.Api.Attributes;

namespace YourGamesList.Api.Model.Requests.Lists;

public class RemoveGameListEntryRequest
{
    [FromAuthorizeHeader] public required UserInformationToken UserInformation { get; init; }
    [FromRoute] public required Guid ListId { get; init; }
    [FromRoute] public required Guid EntryId { get; init; }
}

public class RemoveGameListEntryRequestValidator : AbstractValidator<RemoveGameListEntryRequest>
{
    public RemoveGameListEntryRequestValidator()
    {
        RuleFor(x => x.ListId)
            .NotEmpty();

        RuleFor(x => x.EntryId)
            .NotEmpty();

        RuleFor(x => x.UserInformation)
            .NotEmpty();
    }
}
