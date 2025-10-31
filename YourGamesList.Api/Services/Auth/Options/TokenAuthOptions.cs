using FluentValidation;

namespace YourGamesList.Api.Services.Auth.Options;

//TODO: unit tests
public class TokenAuthOptions
{
    public const string SectionName = "TokenAuth";

    public required string Audience { get; init; }
    public required string Issuer { get; init; }
    public required string JwtSecret { get; init; }
    public required int ExpirationInMinutes { get; init; }
}

public class TokenAuthOptionsValidator : AbstractValidator<TokenAuthOptions>
{
    public TokenAuthOptionsValidator()
    {
        RuleFor(x => x.Audience).NotEmpty();
        RuleFor(x => x.Issuer).NotEmpty();
        RuleFor(x => x.JwtSecret).NotEmpty();
        RuleFor(x => x.ExpirationInMinutes).NotEmpty().GreaterThan(0);
    }
}