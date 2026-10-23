using System;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using YourGamesList.Api.Attributes;
using YourGamesList.Contracts.Requests.Lists;
using YourGamesList.Contracts.Responses;

namespace YourGamesList.Api.Model.Requests.Lists;

public class AddOwnershipInfoRequest
{
    [FromAuthorizeHeader] public required UserInformationToken UserInformation { get; init; }
    [FromRoute] public required Guid ListId { get; init; }
    [FromRoute] public required Guid EntryId { get; init; }
    [FromBody] public required AddOwnershipInfoRequestBody Body { get; init; }
}

public class AddOwnershipInfoRequestValidator : AbstractValidator<AddOwnershipInfoRequest>
{
    public AddOwnershipInfoRequestValidator()
    {
        RuleFor(request => request.ListId).NotEmpty();
        RuleFor(request => request.EntryId).NotEmpty();
        RuleFor(request => request.UserInformation).NotEmpty();

        RuleFor(request => request.Body)
            .NotNull()
            .WithErrorCode(ErrorCodes.Validation.RequestBodyRequired)
            .WithMessage("Proper request body is required.")
            .ChildRules(body =>
            {
                body.RuleFor(request => request.Platform)
                    .IsInEnum()
                    .WithMessage("Invalid platform.");
                body.RuleFor(request => request.GameDistribution)
                    .IsInEnum()
                    .WithMessage("Invalid game distribution.");
                body.RuleFor(request => request.EmulatedOn)
                    .Must(emulator => emulator == null || Enum.IsDefined(emulator.Value))
                    .WithMessage("Invalid emulator.");
                body.RuleFor(request => request.EmulatedOn)
                    .Null()
                    .When(request => !request.WasEmulated)
                    .WithErrorCode(ErrorCodes.Lists.InvalidOwnershipInfoEmulation)
                    .WithMessage("EmulatedOn can only be set when WasEmulated is true.");
            });
    }
}
