using FluentValidation;

namespace YourGamesList.Api.Model.Requests.Auth;

public class UserDeleteRequest
{
    public string Username { get; init; } = "";
    public string Password { get; init; } = "";
}

internal sealed class UserDeleteRequestValidator : AbstractValidator<UserDeleteRequest>
{
    public UserDeleteRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty();

        RuleFor(x => x.Password)
            .NotEmpty();
    }
}