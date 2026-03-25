using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace YourGamesList.Api.Model;

public abstract class BaseToken
{
    public readonly JsonWebToken Jwt;

    public IEnumerable<Claim> Claims => Jwt.Claims;

    public string RawToken => Jwt.EncodedToken;

    protected BaseToken(JsonWebToken jwt)
    {
        Jwt = jwt;
    }

    protected static T CreateFromRawJwt<T>(string rawJwt, Func<JsonWebToken, T> converter) where T : BaseToken
    {
        var handler = new JsonWebTokenHandler();
        JsonWebToken token;
        try
        {
            token = handler.ReadJsonWebToken(rawJwt);
        }
        catch (Exception ex)
        {
            throw BaseTokenCreationException.CannotParseToken(ex);
        }

        return converter(token);
    }

    protected static string GetRequiredClaimOrThrow(IEnumerable<Claim> claims, string claimType)
    {
        var value = claims.FirstOrDefault(x => x.Type == claimType)?.Value;
        return string.IsNullOrEmpty(value)
            ? throw BaseTokenCreationException.MissingOrInvalidClaim(claimType, value)
            : value;
    }
}