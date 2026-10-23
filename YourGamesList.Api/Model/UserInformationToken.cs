using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;
using YourGamesList.Api.Services.Auth;
using YourGamesList.Common;

namespace YourGamesList.Api.Model;

public class UserInformationToken
{
    public required string RawToken { get; init; }
    public required string NormalizedUsername { get; init; }
    public required Guid UserId { get; init; }

    public static CombinedResult<UserInformationToken, UserInformationTokenCreationError> FromHttpContext(HttpContext httpContext)
    {
        var user = httpContext.User;
        if (user.Identity?.IsAuthenticated != true)
        {
            return CombinedResult<UserInformationToken, UserInformationTokenCreationError>.Failure(UserInformationTokenCreationError.NotValid());
        }

        var rawToken = GetRawBearerToken(httpContext.Request);
        if (string.IsNullOrWhiteSpace(rawToken))
        {
            return CombinedResult<UserInformationToken, UserInformationTokenCreationError>.Failure(UserInformationTokenCreationError.RawTokenMissing());
        }

        if (user.Identity?.IsAuthenticated != true)
        {
            return CombinedResult<UserInformationToken, UserInformationTokenCreationError>.Failure(UserInformationTokenCreationError.NotValid());
        }

        var normalizedUsernameClaimReadResult = GetRequiredClaimOrThrow(user, JwtRegisteredClaimNames.Sub);
        if (normalizedUsernameClaimReadResult.IsFailure)
        {
            return CombinedResult<UserInformationToken, UserInformationTokenCreationError>.Failure(normalizedUsernameClaimReadResult.Error);
        }

        var rawUserIdClaimReadResult = GetRequiredClaimOrThrow(user, JwtCustomClaimNames.UserId);
        if (rawUserIdClaimReadResult.IsFailure)
        {
            return CombinedResult<UserInformationToken, UserInformationTokenCreationError>.Failure(rawUserIdClaimReadResult.Error);
        }

        if (!Guid.TryParse(rawUserIdClaimReadResult.Value, out var userId) || userId == Guid.Empty)
        {
            return CombinedResult<UserInformationToken, UserInformationTokenCreationError>.Failure(
                UserInformationTokenCreationError.MissingOrInvalidClaim(JwtCustomClaimNames.UserId, rawUserIdClaimReadResult.Value));
        }

        var userInformationToken = new UserInformationToken()
        {
            RawToken = rawToken,
            UserId = userId,
            NormalizedUsername = normalizedUsernameClaimReadResult.Value
        };
        return CombinedResult<UserInformationToken, UserInformationTokenCreationError>.Success(userInformationToken);
    }

    private static CombinedResult<string, UserInformationTokenCreationError> GetRequiredClaimOrThrow(ClaimsPrincipal user, string claimType)
    {
        var value = user.FindFirst(claimType)?.Value;
        return string.IsNullOrWhiteSpace(value)
            ? CombinedResult<string, UserInformationTokenCreationError>.Failure(UserInformationTokenCreationError.MissingOrInvalidClaim(claimType, value))
            : CombinedResult<string, UserInformationTokenCreationError>.Success(value);
    }

    private static string? GetRawBearerToken(HttpRequest request)
    {
        var authorizationHeader = request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authorizationHeader))
        {
            return null;
        }

        const string bearerPrefix = "Bearer ";
        if (authorizationHeader.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return authorizationHeader[bearerPrefix.Length..].Trim();
        }

        return null;
    }
}