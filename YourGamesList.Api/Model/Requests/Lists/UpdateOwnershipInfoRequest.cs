using System;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using YourGamesList.Api.Attributes;
using YourGamesList.Contracts.Requests.Lists;

namespace YourGamesList.Api.Model.Requests.Lists;

public class UpdateOwnershipInfoRequest
{
    [FromAuthorizeHeader] public required UserInformationToken UserInformation { get; init; }
    [FromRoute] public required Guid ListId { get; init; }
    [FromRoute] public required Guid EntryId { get; init; }
    [FromRoute] public required Guid OwnershipInfoId { get; init; }
    [FromBody] public required UpdateOwnershipInfoRequestBody Body { get; init; }
}

public class UpdateOwnershipInfoRequestValidator : AbstractValidator<UpdateOwnershipInfoRequest>
{
    public UpdateOwnershipInfoRequestValidator()
    {
        RuleFor(request => request.ListId).NotEmpty();
        RuleFor(request => request.EntryId).NotEmpty();
        RuleFor(request => request.OwnershipInfoId).NotEmpty();
        RuleFor(request => request.UserInformation).NotEmpty();

        RuleFor(request => request.Body)
            .NotNull()
            .WithMessage("Proper request body is required.")
            .ChildRules(body =>
            {
                body.RuleFor(request => request.Platform)
                    .Must(platform => platform.Value.HasValue && Enum.IsDefined(platform.Value.Value))
                    .When(request => request.Platform.HasValue)
                    .WithMessage("Invalid platform.");
                body.RuleFor(request => request.GameDistribution)
                    .Must(distribution =>
                        distribution.Value.HasValue && Enum.IsDefined(distribution.Value.Value))
                    .When(request => request.GameDistribution.HasValue)
                    .WithMessage("Invalid game distribution.");
                body.RuleFor(request => request.WasEmulated)
                    .Must(value => value.Value.HasValue)
                    .When(request => request.WasEmulated.HasValue)
                    .WithMessage("WasEmulated must be a boolean.");
                body.RuleFor(request => request.EmulatedOn)
                    .Must(emulator =>
                        emulator.Value == null || Enum.IsDefined(emulator.Value.Value))
                    .When(request => request.EmulatedOn.HasValue)
                    .WithMessage("Invalid emulator.");
            });
    }
}
