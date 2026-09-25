using FluentValidation;

namespace YourGamesList.Api.Services.Auth.Options;

public class UserValuesValidatorOptions
{
    public const string SectionName = "UserValuesValidator";

    public EmailValidatorOptions Email { get; set; } = new();
    public UsernameValidatorOptions Username { get; set; } = new();
    public PasswordValidatorOptions Password { get; set; } = new();
}

public class EmailValidatorOptions
{
    public int MaximumEmailLength { get; init; }
}

public class UsernameValidatorOptions
{
    public int MinimumUsernameLength { get; init; }
    public int MaximumUsernameLength { get; init; }
}

public class PasswordValidatorOptions
{
    public int MinimumPasswordLength { get; init; }
    public int MaximumPasswordLength { get; init; }
}

public class UserValuesValidatorOptionsValidator : AbstractValidator<UserValuesValidatorOptions>
{
    public UserValuesValidatorOptionsValidator()
    {
        RuleFor(x => x.Email).SetValidator(new EmailValidatorOptionsValidator());
        RuleFor(x => x.Username).SetValidator(new UsernameValidatorOptionsValidator());
        RuleFor(x => x.Password).SetValidator(new PasswordValidatorOptionsValidator());
    }
}

public class EmailValidatorOptionsValidator : AbstractValidator<EmailValidatorOptions>
{
    public EmailValidatorOptionsValidator()
    {
        RuleFor(x => x.MaximumEmailLength)
            .NotEmpty()
            .GreaterThan(0);
    }
}

public class UsernameValidatorOptionsValidator : AbstractValidator<UsernameValidatorOptions>
{
    public UsernameValidatorOptionsValidator()
    {
        RuleFor(x => x.MinimumUsernameLength)
            .NotEmpty()
            .GreaterThan(0);
        RuleFor(x => x.MaximumUsernameLength)
            .NotEmpty()
            .GreaterThan(0);

        RuleFor(x => new { x.MinimumUsernameLength, x.MaximumUsernameLength })
            .Must(x => x.MinimumUsernameLength <= x.MaximumUsernameLength)
            .WithMessage("Minimum username length must be less or equal than maximum username length.");
    }
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

        RuleFor(x => new { x.MinimumPasswordLength, x.MaximumPasswordLength })
            .Must(x => x.MinimumPasswordLength <= x.MaximumPasswordLength)
            .WithMessage("Minimum password length must be less or equal than maximum password length.");
    }
}