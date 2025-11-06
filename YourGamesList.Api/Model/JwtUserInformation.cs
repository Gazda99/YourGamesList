using System;
using FluentValidation;

namespace YourGamesList.Api.Model;

public class JwtUserInformation
{
    public required string Username { get; init; }
    public required Guid UserId { get; init; }
}

internal sealed class JwtUserInformationValidator : AbstractValidator<JwtUserInformation>
{
    public JwtUserInformationValidator()
    {
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}