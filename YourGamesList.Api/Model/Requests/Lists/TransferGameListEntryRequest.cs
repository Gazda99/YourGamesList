using System;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using YourGamesList.Api.Attributes;
using YourGamesList.Contracts.Requests.Lists;
using YourGamesList.Contracts.Responses;

namespace YourGamesList.Api.Model.Requests.Lists;

public class TransferGameListEntryRequest
{
    [FromAuthorizeHeader] public required UserInformationToken UserInformation { get; init; }
    [FromRoute] public required Guid ListId { get; init; }
    [FromRoute] public required Guid EntryId { get; init; }
    [FromBody] public required TransferGameListEntryRequestBody Body { get; init; }
}

public class TransferGameListEntryRequestValidator : AbstractValidator<TransferGameListEntryRequest>
{
    public TransferGameListEntryRequestValidator()
    {
        RuleFor(x => x.ListId)
            .NotEmpty();

        RuleFor(x => x.EntryId)
            .NotEmpty();

        RuleFor(x => x.Body)
            .NotNull()
            .WithErrorCode(ErrorCodes.Validation.RequestBodyRequired)
            .WithMessage("Proper request body is required.");

        RuleFor(x => x.Body.TargetListId)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Lists.InvalidTransferTargetList)
            .WithMessage("Target list ID is required.")
            .NotEqual(x => x.ListId)
            .WithErrorCode(ErrorCodes.Lists.InvalidTransferTargetList)
            .WithMessage("Target list must be different from the source list.")
            .When(x => x.Body != null);

        RuleFor(x => x.UserInformation)
            .NotEmpty();
    }
}
