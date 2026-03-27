using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using YourGamesList.Contracts.Requests.Users;

namespace YourGamesList.Api.Model.Requests.Auth;

public class UserRegisterRequest
{
    [FromBody] public required AuthUserRegisterRequestBody Body { get; init; }
}

internal sealed class UserRegisterRequestValidator : AbstractValidator<UserRegisterRequest>
{
    public UserRegisterRequestValidator()
    {
        RuleFor(x => x.Body.Username)
            .NotEmpty();

        RuleFor(x => x.Body.Password)
            .NotEmpty();
    }
}