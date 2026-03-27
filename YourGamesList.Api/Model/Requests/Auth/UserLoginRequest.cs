using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using YourGamesList.Contracts.Requests.Users;

namespace YourGamesList.Api.Model.Requests.Auth;

public class UserLoginRequest
{
    [FromBody] public required AuthUserLoginRequestBody Body { get; init; }
}

internal sealed class UserLoginRequestValidator : AbstractValidator<UserLoginRequest>
{
    public UserLoginRequestValidator()
    {
        RuleFor(x => x.Body.Username)
            .NotEmpty();

        RuleFor(x => x.Body.Password)
            .NotEmpty();
    }
}