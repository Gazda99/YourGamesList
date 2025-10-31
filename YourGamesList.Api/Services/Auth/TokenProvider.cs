using System;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using YourGamesList.Api.Services.Auth.Options;

namespace YourGamesList.Api.Services.Auth;

public interface ITokenProvider
{
    string CreateToken(string username, Guid userId);
}

//TODO: unit tests

public class TokenProvider : ITokenProvider
{
    private readonly IOptions<TokenAuthOptions> _options;
    private readonly TimeProvider _timeProvider;

    public TokenProvider(IOptions<TokenAuthOptions> options, TimeProvider timeProvider)
    {
        _options = options;
        _timeProvider = timeProvider;
    }

    public string CreateToken(string username, Guid userId)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Value.JwtSecret));

        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var time = _timeProvider.GetUtcNow().UtcDateTime;
        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(
                [
                    new Claim(JwtRegisteredClaimNames.Sub, username),
                    new Claim(JwtCustomClaimNames.UserId, userId.ToString()),
                    new Claim(JwtRegisteredClaimNames.Aud, _options.Value.Audience),
                ]
            ),
            NotBefore = time,
            Expires = time.AddMinutes(_options.Value.ExpirationInMinutes),
            SigningCredentials = credentials,
            Issuer = _options.Value.Issuer,
            Audience = _options.Value.Audience
        };

        var handler = new JsonWebTokenHandler();
        var token = handler.CreateToken(tokenDescriptor);

        return token;
    }
}