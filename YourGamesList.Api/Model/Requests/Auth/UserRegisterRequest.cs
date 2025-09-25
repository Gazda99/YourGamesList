using FluentValidation;

namespace YourGamesList.Api.Model.Requests.Auth;

//TODO: This is only a placeholder for now
public class UserRegisterRequest
{
    public string? Username { get; init; }
    public string? Password { get; init; }
}

internal sealed class UserRegisterRequestValidator : AbstractValidator<UserRegisterRequest>
{
    public UserRegisterRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty();

        RuleFor(x => x.Password)
            .NotEmpty();
    }
}