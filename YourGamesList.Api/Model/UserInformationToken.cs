using System;
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using YourGamesList.Api.Services.Auth;
using YourGamesList.Common;

namespace YourGamesList.Api.Model;

public class UserInformationToken
{
    public string Username { get; }
    public Guid UserId { get; }

    public UserInformationToken(string username, Guid userId)
    {
        Username = username;
        UserId = userId;
    }

    public static CombinedResult<UserInformationToken, UserInformationTokenCreationError> FromClaimsPrincipal(ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return CombinedResult<UserInformationToken, UserInformationTokenCreationError>.Failure(UserInformationTokenCreationError.NotValid());
        }

        var usernameClaimReadResult = GetRequiredClaimOrThrow(user, JwtRegisteredClaimNames.Sub);
        if (usernameClaimReadResult.IsFailure)
        {
            return CombinedResult<UserInformationToken, UserInformationTokenCreationError>.Failure(usernameClaimReadResult.Error);
        }

        if (string.IsNullOrWhiteSpace(usernameClaimReadResult.Value))
        {
            return CombinedResult<UserInformationToken, UserInformationTokenCreationError>.Failure(UserInformationTokenCreationError.MissingOrInvalidClaim(JwtRegisteredClaimNames.Sub, usernameClaimReadResult.Value));
        }

        var rawUserIdClaimReadResult = GetRequiredClaimOrThrow(user, JwtCustomClaimNames.UserId);
        if (rawUserIdClaimReadResult.IsFailure)
        {
            return CombinedResult<UserInformationToken, UserInformationTokenCreationError>.Failure(rawUserIdClaimReadResult.Error);
        }

        if (!Guid.TryParse(rawUserIdClaimReadResult.Value, out var userId) || userId == Guid.Empty)
        {
            return CombinedResult<UserInformationToken, UserInformationTokenCreationError>.Failure(UserInformationTokenCreationError.MissingOrInvalidClaim(JwtCustomClaimNames.UserId, rawUserIdClaimReadResult.Value));
        }

        var userInformationToken = new UserInformationToken(usernameClaimReadResult.Value, userId);
        return CombinedResult<UserInformationToken, UserInformationTokenCreationError>.Success(userInformationToken);
    }

    private static CombinedResult<string, UserInformationTokenCreationError> GetRequiredClaimOrThrow(ClaimsPrincipal user, string claimType)
    {
        var value = user.FindFirst(claimType)?.Value;
        return string.IsNullOrEmpty(value)
            ? CombinedResult<string, UserInformationTokenCreationError>.Failure(UserInformationTokenCreationError.MissingOrInvalidClaim(claimType, value))
            : CombinedResult<string, UserInformationTokenCreationError>.Success(value);
    }
}