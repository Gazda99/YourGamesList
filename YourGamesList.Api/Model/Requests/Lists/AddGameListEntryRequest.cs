using System;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using YourGamesList.Api.Attributes;
using YourGamesList.Api.Services.Auth.Options;
using YourGamesList.Contracts.Requests.Lists;
using YourGamesList.Contracts.Responses;

namespace YourGamesList.Api.Model.Requests.Lists;

public class AddGameListEntryRequest
{
    [FromAuthorizeHeader] public required UserInformationToken UserInformation { get; init; }
    [FromRoute] public required Guid ListId { get; init; }
    [FromBody] public required AddGameListEntryRequestBody Body { get; init; }
}

public class AddGameListEntryRequestValidator : AbstractValidator<AddGameListEntryRequest>
{
    public AddGameListEntryRequestValidator(IOptions<RequestValidatorOptions> options)
    {
        var maximumDescriptionLength = options.Value.Lists.MaximumGameListEntryDescriptionLength;
        var minimumRating = options.Value.Lists.MinimumGameListEntryRating;
        var maximumRating = options.Value.Lists.MaximumGameListEntryRating;

        RuleFor(x => x.ListId)
            .NotEmpty();

        RuleFor(x => x.Body)
            .NotNull()
            .WithErrorCode(ErrorCodes.Validation.RequestBodyRequired)
            .WithMessage("Proper request body is required.")
            .ChildRules(body =>
            {
                body.RuleFor(x => x.CompletionStatus)
                    .IsInEnum()
                    .WithErrorCode(ErrorCodes.Lists.InvalidGameListEntryCompletionStatus)
                    .WithMessage("Invalid game completion status.");
                body.RuleFor(x => x.IgdbId)
                    .GreaterThan(0)
                    .WithErrorCode(ErrorCodes.Lists.InvalidGameListEntryIgdbId)
                    .WithMessage("IGDB ID must be greater than zero.");
                body.RuleFor(x => x.Description)
                    .MaximumLength(maximumDescriptionLength)
                    .WithErrorCode(ErrorCodes.Lists.InvalidGameListEntryDescription)
                    .WithMessage($"Description must be no longer than {maximumDescriptionLength} characters.");
                body.RuleFor(x => x.Rating)
                    .Must(rating => rating >= minimumRating && rating <= maximumRating)
                    .When(x => x.Rating.HasValue)
                    .WithErrorCode(ErrorCodes.Lists.InvalidGameListEntryRating)
                    .WithMessage($"Rating must be between {minimumRating} and {maximumRating}.");
            });

        RuleFor(x => x.UserInformation)
            .NotEmpty();
    }
}
