using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using AutoFixture;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace YourGamesList.TestsUtils;

public static class JwtHelper
{
    private static readonly IFixture Fixture = new Fixture();

    public static (string rawToken, JsonWebToken token) CreateToken(
        IEnumerable<Claim>? claims = null,
        string? jwtSecret = null,
        DateTime? now = null,
        int expirationInMinutes = 30,
        string? issuer = null,
        string? audience = null
    )
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret ?? new string(Fixture.CreateMany<char>(256).ToArray())));

        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var time = now ?? DateTime.UtcNow;
        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(
                claims ?? []
            ),
            NotBefore = time,
            Expires = time.AddMinutes(expirationInMinutes),
            SigningCredentials = credentials,
            Issuer = issuer ?? Fixture.Create<string>(),
            Audience = audience ?? Fixture.Create<string>(),
        };

        var handler = new JsonWebTokenHandler();
        var rawToken = handler.CreateToken(tokenDescriptor);
        var token = handler.ReadJsonWebToken(rawToken);

        return (rawToken, token);
    }
}