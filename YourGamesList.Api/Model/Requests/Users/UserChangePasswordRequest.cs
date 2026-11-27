using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YourGamesList.Api.Attributes;
using YourGamesList.Api.Services.Auth.Options;
using YourGamesList.Contracts.Requests.Users;
using YourGamesList.Contracts.Responses;

namespace YourGamesList.Api.Model.Requests.Users;

public class UserChangePasswordRequest
{
    [FromAuthorizeHeader] public required UserInformationToken UserInformation { get; init; }
    [FromBody] public required AuthUserChangePasswordRequestBody Body { get; init; }
}

public class UserChangePasswordRequestValidator : AbstractValidator<UserChangePasswordRequest>
{
    public UserChangePasswordRequestValidator(
        ILogger<UserChangePasswordRequestValidator> logger,
        IOptions<RequestValidatorOptions> options
    )
    {
        var validatorOptions = options.Value;

        RuleFor(x => x.UserInformation)
            .NotEmpty();

        RuleFor(x => x.Body)
            .NotNull()
            .WithErrorCode(ErrorCodes.Validation.RequestBodyRequired)
            .WithMessage("Proper request body is required.")
            .ChildRules(body =>
            {
                body.RuleFor(x => x.CurrentPassword)
                    .NotEmpty()
                    .WithErrorCode(ErrorCodes.UserAuth.InvalidPassword)
                    .WithMessage("Current password is required.");

                body.RuleFor(x => x.NewPassword)
                    .MustBeValidNewPassword(validatorOptions.Password, logger)
                    .Must((b, newPassword) => newPassword != b.CurrentPassword)
                    .WithErrorCode(ErrorCodes.UserAuth.NewPasswordSameAsCurrent)
                    .WithMessage("New password must be different from the current password.");
            });
    }
}
