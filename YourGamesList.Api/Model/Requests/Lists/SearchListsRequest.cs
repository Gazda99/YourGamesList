using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using YourGamesList.Api.Attributes;
using YourGamesList.Api.Services.Auth.Options;
using YourGamesList.Contracts.Responses;

namespace YourGamesList.Api.Model.Requests.Lists;

public class SearchListsRequest
{
    [FromAuthorizeHeader] public required UserInformationToken UserInformation { get; init; }
    [FromQuery] public string? OwnerName { get; init; }
    [FromQuery] public string? ListName { get; init; }
    [FromQuery] public bool IncludeEntries { get; init; }
}

public class SearchListsRequestValidator : AbstractValidator<SearchListsRequest>
{
    public SearchListsRequestValidator(IOptions<RequestValidatorOptions> options)
    {
        var minimumUsernameLength = options.Value.Username.MinimumUsernameLength;
        var minimumListNameLength = options.Value.Lists.MinimumListNameLength;

        RuleFor(request => request)
            .Must(request =>
                !string.IsNullOrWhiteSpace(request.OwnerName) ||
                !string.IsNullOrWhiteSpace(request.ListName))
            .WithErrorCode(ErrorCodes.Lists.InvalidListSearchCriteria)
            .WithMessage("At least one of ownerName or listName must be provided.");

        RuleFor(request => request.ListName)
            .Must(listName => listName!.Trim().Length >= minimumListNameLength)
            .When(request => request.ListName is not null)
            .WithErrorCode(ErrorCodes.Lists.InvalidListSearchCriteria)
            .WithMessage($"List name search term must be at least {minimumListNameLength} characters.");

        RuleFor(request => request.OwnerName)
            .Must(ownerName => ownerName!.Trim().Length >= minimumUsernameLength)
            .When(request => request.OwnerName is not null)
            .WithErrorCode(ErrorCodes.Lists.InvalidListSearchCriteria)
            .WithMessage($"Owner name search term must be at least {minimumUsernameLength} characters.");

        RuleFor(request => request.UserInformation)
            .NotEmpty();
    }
}
