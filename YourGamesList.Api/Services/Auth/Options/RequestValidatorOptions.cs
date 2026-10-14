using FluentValidation;

namespace YourGamesList.Api.Services.Auth.Options;

public class RequestValidatorOptions
{
    public const string SectionName = "RequestValidator";

    public EmailValidatorOptions Email { get; set; } = new();
    public ListsValidatorOptions Lists { get; set; } = new();
    public UsernameValidatorOptions Username { get; set; } = new();
    public PasswordValidatorOptions Password { get; set; } = new();
    public DateOfBirthValidatorOptions DateOfBirth { get; set; } = new();
}

public class EmailValidatorOptions
{
    public int MaximumEmailLength { get; init; }
}

public class ListsValidatorOptions
{
    public int MaximumListNameLength { get; init; }
}

public class UsernameValidatorOptions
{
    public string AllowedCharactersRegex { get; init; } = "";
    public int MinimumUsernameLength { get; init; }
    public int MaximumUsernameLength { get; init; }
}

public class PasswordValidatorOptions
{
    public int MinimumPasswordLength { get; init; }
    public int MaximumPasswordLength { get; init; }
}

public class DateOfBirthValidatorOptions
{
    public int MinimumAge { get; init; }
    public int MaximumAge { get; init; }
}

public class UserValuesValidatorOptionsValidator : AbstractValidator<RequestValidatorOptions>
{
    public UserValuesValidatorOptionsValidator()
    {
        RuleFor(x => x.Email).SetValidator(new EmailValidatorOptionsValidator());
        RuleFor(x => x.Lists).SetValidator(new ListsValidatorOptionsValidator());
        RuleFor(x => x.Username).SetValidator(new UsernameValidatorOptionsValidator());
        RuleFor(x => x.Password).SetValidator(new PasswordValidatorOptionsValidator());
        RuleFor(x => x.DateOfBirth).SetValidator(new DateOfBirthValidatorOptionsValidator());
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

public class ListsValidatorOptionsValidator : AbstractValidator<ListsValidatorOptions>
{
    public ListsValidatorOptionsValidator()
    {
        RuleFor(x => x.MaximumListNameLength)
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

        RuleFor(x => x.AllowedCharactersRegex)
            .NotEmpty();
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

public class DateOfBirthValidatorOptionsValidator : AbstractValidator<DateOfBirthValidatorOptions>
{
    public DateOfBirthValidatorOptionsValidator()
    {
        RuleFor(x => x.MinimumAge)
            .NotEmpty()
            .GreaterThan(0);

        RuleFor(x => x.MaximumAge)
            .NotEmpty()
            .GreaterThan(0);
        
        RuleFor(x => new { x.MinimumAge, x.MaximumAge })
            .Must(x => x.MinimumAge <= x.MaximumAge)
            .WithMessage("Minimum age must be less or equal than maximum age.");
    }
}