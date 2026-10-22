using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YourGamesList.Api.Attributes;
using YourGamesList.Api.Services.Auth.Options;
using YourGamesList.Contracts.Requests.Lists;
using YourGamesList.Contracts.Responses;

namespace YourGamesList.Api.Model.Requests.Lists;

public class AddListRequest
{
    [FromAuthorizeHeader] public required UserInformationToken UserInformation { get; init; }
    [FromBody] public required CreateListRequestBody Body { get; init; }
}

public class CreateListRequestValidator : AbstractValidator<AddListRequest>
{
    public CreateListRequestValidator(
        ILogger<CreateListRequestValidator> logger,
        IOptions<RequestValidatorOptions> options
        )
    {
        var validatorOptions = options.Value;
        
        RuleFor(x => x.Body)
            .NotNull()
            .WithErrorCode(ErrorCodes.Validation.RequestBodyRequired)
            .WithMessage("Proper request body is required.")
            .ChildRules(body =>
            {
                body.RuleFor(x => x.Name)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                    .WithErrorCode(ErrorCodes.Lists.InvalidListName)
                    .WithMessage("Invalid list name.")
                    .Must(listName => NormalizeListName(listName).Length >= validatorOptions.Lists.MinimumListNameLength)
                    .WithErrorCode(ErrorCodes.Lists.InvalidListName)
                    .WithMessage($"List name must be at least {validatorOptions.Lists.MinimumListNameLength} characters.")
                    .Must(listName =>
                    {
                        var listNameNormalized = NormalizeListName(listName);
                        if (listNameNormalized.Length > validatorOptions.Lists.MaximumListNameLength)
                        {
                            logger.LogInformation("List name {ListName} is too long. {ListNameLength} > {ListNameMaximumLength} ", listNameNormalized, listNameNormalized.Length, validatorOptions.Lists.MaximumListNameLength);
                            return false;
                        }
                        return true;
                    })
                    .WithErrorCode(ErrorCodes.Lists.ListNameTooLong)
                    .WithMessage($"List name must be no longer than {validatorOptions.Lists.MaximumListNameLength} characters.")
                    ;
            });
        
        RuleFor(x => x.UserInformation)
            .NotEmpty();
        
        return;

        static string NormalizeListName(string listName)
        {
            return listName.Trim();
        }

    }
}