using FluentValidation;

namespace YourGamesList.Api.Services.Auth.Options;

public class UserValuesValidatorOptions
{
    public const string SectionName = "UserValuesValidator";

    public int MinimumPasswordLength { get; init; }
    public int MaximumPasswordLength { get; init; }
}

public class PasswordValidatorOptionsValidator : AbstractValidator<UserValuesValidatorOptions>
{
    public PasswordValidatorOptionsValidator()
    {
        RuleFor(x => x.MinimumPasswordLength)
            .NotEmpty()
            .GreaterThan(0);
        RuleFor(x => x.MaximumPasswordLength)
            .NotEmpty()
            .GreaterThan(0);

        RuleFor(x => new { x.MinimumPasswordLength, x.MaximumPasswordLength })
            .Must(x => x.MinimumPasswordLength <= x.MaximumPasswordLength)
            .WithMessage("Minimum password length must be less or equal than maximum password length.");
    }
}