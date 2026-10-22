using System;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using YourGamesList.Api.Attributes;
using YourGamesList.Api.Services.Auth.Options;
using YourGamesList.Contracts.Requests.Lists;
using YourGamesList.Contracts.Responses;

namespace YourGamesList.Api.Model.Requests.Lists;

public class UpdateGameListEntryRequest
{
    [FromAuthorizeHeader] public required UserInformationToken UserInformation { get; init; }
    [FromRoute] public required Guid ListId { get; init; }
    [FromRoute] public required Guid EntryId { get; init; }
    [FromBody] public required UpdateGameListEntryRequestBody Body { get; init; }
}

public class UpdateGameListEntryRequestValidator : AbstractValidator<UpdateGameListEntryRequest>
{
    public UpdateGameListEntryRequestValidator(IOptions<RequestValidatorOptions> options)
    {
        var maximumDescriptionLength = options.Value.Lists.MaximumGameListEntryDescriptionLength;

        RuleFor(x => x.ListId)
            .NotEmpty();

        RuleFor(x => x.EntryId)
            .NotEmpty();

        RuleFor(x => x.Body)
            .NotNull()
            .WithErrorCode(ErrorCodes.Validation.RequestBodyRequired)
            .WithMessage("Proper request body is required.")
            .ChildRules(body =>
            {
                body.RuleFor(x => x.CompletionStatus)
                    .Must(status => status.Value.HasValue && Enum.IsDefined(status.Value.Value))
                    .When(x => x.CompletionStatus.HasValue)
                    .WithErrorCode(ErrorCodes.Lists.InvalidGameListEntryCompletionStatus)
                    .WithMessage("Invalid game completion status.");

                body.RuleFor(x => x.Description)
                    .Must(description =>
                        description.Value == null ||
                        description.Value.Length <= maximumDescriptionLength)
                    .When(x => x.Description.HasValue)
                    .WithErrorCode(ErrorCodes.Lists.InvalidGameListEntryDescription)
                    .WithMessage($"Description must be no longer than {maximumDescriptionLength} characters, or null.");
            });

        RuleFor(x => x.UserInformation)
            .NotEmpty();
    }
}
