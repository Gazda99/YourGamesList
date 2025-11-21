using FluentValidation;

namespace YourGamesList.Web.Page.Services.UserState.Options;

public class UserLoginStateManagerOptions
{
    public const string SectionName = "UserLoginStateManager";
    public int TokenTtlInMinutes { get; init; }
}

internal sealed class UserLoginStateManagerOptionsValidator : AbstractValidator<UserLoginStateManagerOptions>
{
    public UserLoginStateManagerOptionsValidator()
    {
        RuleFor(x => x.TokenTtlInMinutes)
            .NotEmpty()
            .GreaterThan(0);
    }
}