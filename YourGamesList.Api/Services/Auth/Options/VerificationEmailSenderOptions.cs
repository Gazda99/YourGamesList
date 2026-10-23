using FluentValidation;
using YourGamesList.Common.Options.Validators;

namespace YourGamesList.Api.Services.Auth.Options;

public class VerificationEmailSenderOptions
{
    public const string SectionName = "VerificationEmailSender";

    public required string YglUrl { get; set; }
    public required string Address { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string SmtpHost { get; set; }
    public required int SmtpPort { get; set; } = 465;
}

public class VerificationEmailSenderOptionsValidator : AbstractValidator<VerificationEmailSenderOptions>
{
    public VerificationEmailSenderOptionsValidator()
    {
        RuleFor(x => x.YglUrl)
            .NotEmpty()
            .IsValidAbsoluteUrl();
        RuleFor(x => x.Password)
            .NotEmpty();
        RuleFor(x => x.Address)
            .NotEmpty();
        RuleFor(x => x.Username)
            .NotEmpty();
        RuleFor(x => x.SmtpHost)
            .NotEmpty();
    }
}