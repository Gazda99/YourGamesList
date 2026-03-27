using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using YourGamesList.Contracts.Requests.Users;

namespace YourGamesList.Api.Model.Requests.Auth;

public class UserDeleteRequest
{
    [FromBody] public required AuthUserDeleteRequestBody Body { get; init; }
}

internal sealed class UserDeleteRequestValidator : AbstractValidator<UserDeleteRequest>
{
    public UserDeleteRequestValidator()
    {
        RuleFor(x => x.Body.Username)
            .NotEmpty();

        RuleFor(x => x.Body.Password)
            .NotEmpty();
    }
}