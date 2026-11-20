using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using YourGamesList.Contracts.Requests.Users;
using YourGamesList.Contracts.Responses;

namespace YourGamesList.Api.Model.Requests.Users;

public class ConfirmEmailRequest
{
    [FromBody] public required AuthConfirmEmailRequestBody Body { get; init; }
}

public class ConfirmEmailRequestValidator : AbstractValidator<ConfirmEmailRequest>
{
    public ConfirmEmailRequestValidator()
    {
        RuleFor(x => x.Body)
            .NotNull()
            .WithErrorCode(ErrorCodes.Validation.RequestBodyRequired)
            .WithMessage("Proper request body is required.")
            .ChildRules(body =>
            {
                body.RuleFor(x => x.Token)
                    .NotEmpty()
                    .WithErrorCode(ErrorCodes.UserAuth.EmailVerificationInvalid)
                    .WithMessage("Email verification token is required.");
            });
    }
}