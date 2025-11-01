using FluentValidation;

namespace YourGamesList.Api.Model;

public class JwtUserInformation
{
    public required string Username { get; init; }
    public required string UserId { get; init; }
}

internal sealed class JwtUserInformationValidator : AbstractValidator<JwtUserInformation>
{
    public JwtUserInformationValidator()
    {
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}