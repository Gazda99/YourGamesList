using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using YourGamesList.Contracts.Requests.Users;

namespace YourGamesList.Api.Model.Requests.Users;

public class UserDeleteRequest
{
    [FromBody] public required AuthUserDeleteRequestBody Body { get; init; }
}

public class UserDeleteRequestValidator : AbstractValidator<UserDeleteRequest>
{
    public UserDeleteRequestValidator()
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