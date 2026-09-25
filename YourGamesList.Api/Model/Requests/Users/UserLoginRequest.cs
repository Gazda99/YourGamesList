using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using YourGamesList.Contracts.Requests.Users;

namespace YourGamesList.Api.Model.Requests.Users;

public class UserLoginRequest
{
    [FromBody] public required AuthUserLoginRequestBody Body { get; init; }
}

public class UserLoginRequestValidator : AbstractValidator<UserLoginRequest>
{
    public UserLoginRequestValidator()
    {
        RuleFor(x => x.Body)
            .NotEmpty()
            .WithMessage("Proper request body is required.")
            .ChildRules(body =>
            {
                body.RuleFor(x => x.Username)
                    .NotEmpty();

                body.RuleFor(x => x.Password)
                    .NotEmpty();
            });
    }
}