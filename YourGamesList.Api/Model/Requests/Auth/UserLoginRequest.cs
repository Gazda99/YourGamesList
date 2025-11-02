using FluentValidation;

namespace YourGamesList.Api.Model.Requests.Auth;

public class UserLoginRequest
{
    public string Username { get; init; } = "";
    public string Password { get; init; } = "";
}

internal sealed class UserLoginRequestValidator : AbstractValidator<UserLoginRequest>
{
    public UserLoginRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty();

        RuleFor(x => x.Password)
            .NotEmpty();
    }
}