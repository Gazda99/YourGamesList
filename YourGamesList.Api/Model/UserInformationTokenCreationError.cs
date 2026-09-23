using System;

namespace YourGamesList.Api.Model;

public class UserInformationTokenCreationError
{
    public string Message { get; private set; }
    public Exception? Exception { get; private set; }

    public UserInformationTokenCreationError(string message)
    {
        Message = message;
    }

    public UserInformationTokenCreationError(string message, Exception ex)
    {
        Message = message;
        Exception = ex;
    }

    public static UserInformationTokenCreationError NotValid()
    {
        return new UserInformationTokenCreationError("Provided token is not valid.");
    }

    public static UserInformationTokenCreationError MissingOrInvalidClaim(string claimName, string? claimValue)
    {
        return new UserInformationTokenCreationError($"Required claim '{claimName}' is missing or invalid. Value: '{claimValue ?? "N/a"}'.");
    }
}