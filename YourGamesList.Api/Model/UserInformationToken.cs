using System;
using FluentValidation;
using Microsoft.IdentityModel.JsonWebTokens;
using YourGamesList.Api.Services.Auth;

namespace YourGamesList.Api.Model;

public class UserInformationToken : BaseToken
{
    public string Username { get; private set; }
    public Guid UserId { get; private set; }

    public UserInformationToken(JsonWebToken jwt) : base(jwt)
    {
        Username = GetRequiredClaimOrThrow(jwt.Claims, JwtRegisteredClaimNames.Sub);
        if (string.IsNullOrWhiteSpace(Username))
        {
            throw BaseTokenCreationException.MissingOrInvalidClaim(JwtRegisteredClaimNames.Sub, Username);
        }

        var rawUserId = GetRequiredClaimOrThrow(jwt.Claims, JwtCustomClaimNames.UserId);

        if (!Guid.TryParse(rawUserId, out var userId))
        {
            throw BaseTokenCreationException.MissingOrInvalidClaim(JwtCustomClaimNames.UserId, rawUserId);
        }

        UserId = userId;
    }

    public static UserInformationToken FromJwt(JsonWebToken jwt)
    {
        return new UserInformationToken(jwt);
    }

    public static UserInformationToken FromRawJwt(string rawJwt)
    {
        return CreateFromRawJwt(rawJwt, FromJwt);
    }
}

internal sealed class UserInformationTokenValidator : AbstractValidator<UserInformationToken>
{
    public UserInformationTokenValidator()
    {
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}