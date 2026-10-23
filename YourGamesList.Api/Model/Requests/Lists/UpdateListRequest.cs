using System;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YourGamesList.Api.Attributes;
using YourGamesList.Api.Services.Auth.Options;
using YourGamesList.Contracts.Requests.Lists;
using YourGamesList.Contracts.Responses;

namespace YourGamesList.Api.Model.Requests.Lists;

public class UpdateListRequest
{
    [FromAuthorizeHeader] public required UserInformationToken UserInformation { get; init; }
    [FromRoute] public required Guid ListId { get; init; }
    [FromBody] public required UpdateListRequestBody Body { get; init; }
}

public class UpdateListRequestValidator : AbstractValidator<UpdateListRequest>
{
    public UpdateListRequestValidator(
        ILogger<UpdateListRequestValidator> logger,
        IOptions<RequestValidatorOptions> options)
    {
        var maximumListNameLength = options.Value.Lists.MaximumListNameLength;
        var minimumListNameLength = options.Value.Lists.MinimumListNameLength;

        RuleFor(x => x.ListId)
            .NotEmpty();

        RuleFor(x => x.Body)
            .NotNull()
            .WithErrorCode(ErrorCodes.Validation.RequestBodyRequired)
            .WithMessage("Proper request body is required.")
            .ChildRules(body =>
            {
                body.RuleFor(x => x.Name)
                    .Must(name => name.Value != null)
                    .When(x => x.Name.HasValue, ApplyConditionTo.CurrentValidator)
                    .WithErrorCode(ErrorCodes.Lists.InvalidListName)
                    .WithMessage("List name must be a string.")
                    .Must(name =>
                        {
                            var normalizedName = name.Value!.Trim();
                            if (normalizedName.Length < minimumListNameLength)
                            {
                                logger.LogInformation("List name {ListName} is too short. {ListNameLength} < {ListNameMaximumLength}", normalizedName,
                                    normalizedName.Length, minimumListNameLength);
                                return false;
                            }

                            return true;
                        }
                     )
                    .When(x => x.Name.HasValue && x.Name.Value != null, ApplyConditionTo.CurrentValidator)
                    .WithErrorCode(ErrorCodes.Lists.InvalidListName)
                    .WithMessage($"List name must be at least {minimumListNameLength} characters.")
                    .Must(name =>
                    {
                        var normalizedName = name.Value!.Trim();
                        if (normalizedName.Length > maximumListNameLength)
                        {
                            logger.LogInformation("List name {ListName} is too long. {ListNameLength} > {ListNameMaximumLength}", normalizedName, normalizedName.Length, maximumListNameLength);
                            return false;
                        }

                        return true;
                    })
                    .When(x => x.Name.HasValue && x.Name.Value != null, ApplyConditionTo.CurrentValidator)
                    .WithErrorCode(ErrorCodes.Lists.ListNameTooLong)
                    .WithMessage($"List name must be no longer than {maximumListNameLength} characters.");

                body.RuleFor(x => x.Description)
                    .Must(description => description.Value == null || description.Value.Length <= 500)
                    .When(x => x.Description.HasValue)
                    .WithErrorCode(ErrorCodes.Lists.InvalidListDescription)
                    .WithMessage("Description must be a string no longer than 500 characters, or null.");

                body.RuleFor(x => x.IsPrivate)
                    .Must(isPrivate => isPrivate.Value.HasValue)
                    .When(x => x.IsPrivate.HasValue)
                    .WithErrorCode(ErrorCodes.Lists.InvalidListPrivacy)
                    .WithMessage("IsPrivate must be a boolean.");
            });

        RuleFor(x => x.UserInformation)
            .NotEmpty();
    }
}