using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using YourGamesList.Contracts.Requests.Users;

namespace YourGamesList.Api.Model.Requests.Users;

public class UserRegisterRequest
{
    [FromBody] public required AuthUserRegisterRequestBody Body { get; init; }
}

public class UserRegisterRequestValidator : AbstractValidator<UserRegisterRequest>
{
    public UserRegisterRequestValidator()
    {
        RuleFor(x => x.Body)
            .NotEmpty()
            .WithMessage("Proper request body is required.")
            .ChildRules(body =>
            {
                body.RuleFor(x => x.Username)
                    .NotEmpty();

                body.RuleFor(x => x.Email)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                    .EmailAddress();

                body.RuleFor(x => x.Password)
                    .NotEmpty();
                
                body.RuleFor(x => x.DateOfBirth)
                    .NotEmpty();
            });
    }
}