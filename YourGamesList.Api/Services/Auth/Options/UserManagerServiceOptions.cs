using FluentValidation;

namespace YourGamesList.Api.Services.Auth.Options;

public class UserManagerServiceOptions
{
    public const string SectionName = "UserManagerService";

    public bool EnableEmailConfirmation { get; set; }
    public int DefaultEmailVerificationExpiryInMinutes { get; set; }
    public int DefaultPendingUserExpiryInMinutes { get; set; }
}

public class UserManagerServiceOptionsValidator : AbstractValidator<UserManagerServiceOptions>
{
    public UserManagerServiceOptionsValidator()
    {
        RuleFor(x => x.DefaultEmailVerificationExpiryInMinutes)
            .NotEmpty()
            .GreaterThan(0);

        RuleFor(x => x.DefaultPendingUserExpiryInMinutes)
            .NotEmpty()
            .GreaterThan(0);
    }
}