using System;

namespace YourGamesList.Api.Model;

public class BaseTokenCreationException : Exception
{
    public BaseTokenCreationException(string message) : base(message)
    {
    }

    public BaseTokenCreationException(string message, Exception ex) : base(message, ex)
    {
    }

    public static BaseTokenCreationException CannotParseToken(Exception ex)
    {
        return new BaseTokenCreationException("Provided token cannot be parsed to JWT.", ex);
    }

    public static BaseTokenCreationException MissingOrInvalidClaim(string claimName, string? claimValue)
    {
        return new BaseTokenCreationException($"Required claim '{claimName}' is missing or invalid. Value: '{claimValue ?? "N/a"}'.");
    }
}