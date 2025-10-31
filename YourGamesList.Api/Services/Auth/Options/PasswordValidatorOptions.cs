using FluentValidation;

namespace YourGamesList.Api.Services.Auth.Options;

//TODO: unit tests
public class PasswordValidatorOptions
{
    public const string SectionName = "PasswordValidator";

    public int MinimumPasswordLength { get; init; }
    public int MaximumPasswordLength { get; init; }
}

public class PasswordValidatorOptionsValidator : AbstractValidator<PasswordValidatorOptions>
{
    public PasswordValidatorOptionsValidator()
    {
        RuleFor(x => x.MinimumPasswordLength)
            .NotEmpty()
            .GreaterThan(0);
        RuleFor(x => x.MaximumPasswordLength)
            .NotEmpty()
            .GreaterThan(0);
    }
}