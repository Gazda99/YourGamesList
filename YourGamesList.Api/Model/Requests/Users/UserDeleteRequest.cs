using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using YourGamesList.Api.Attributes;
using YourGamesList.Contracts.Requests.Users;
using YourGamesList.Contracts.Responses;

namespace YourGamesList.Api.Model.Requests.Users;

public class UserDeleteRequest
{
    [FromAuthorizeHeader] public required UserInformationToken UserInformation { get; init; }
    [FromBody] public required AuthUserDeleteRequestBody Body { get; init; }
}

public class UserDeleteRequestValidator : AbstractValidator<UserDeleteRequest>
{
    public UserDeleteRequestValidator()
    {
        RuleFor(x => x.UserInformation)
            .NotEmpty();

        RuleFor(x => x.Body)
            .NotNull()
            .WithErrorCode(ErrorCodes.Validation.RequestBodyRequired)
            .WithMessage("Proper request body is required.")
            .ChildRules(body =>
            {
                body.RuleFor(x => x.Username)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                    .WithErrorCode(ErrorCodes.UserAuth.InvalidUsername)
                    .WithMessage("Username is required.");

                body.RuleFor(x => x.Password)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                    .WithErrorCode(ErrorCodes.UserAuth.InvalidPassword)
                    .WithMessage("Password is required.");
            });
    }
}