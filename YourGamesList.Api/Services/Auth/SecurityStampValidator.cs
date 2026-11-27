using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YourGamesList.Database;

namespace YourGamesList.Api.Services.Auth;

public interface ISecurityStampValidator
{
    /// <summary>
    /// Checks that the token's security stamp matches the current stamp of its user.
    /// Returns false when the user no longer exists, the claims are missing, or the stamp is stale.
    /// </summary>
    Task<bool> IsValid(ClaimsPrincipal principal);
}

public class SecurityStampValidator : ISecurityStampValidator
{
    private readonly ILogger<SecurityStampValidator> _logger;
    private readonly YourGamesListDbContext _yglDbContext;

    public SecurityStampValidator(ILogger<SecurityStampValidator> logger, YourGamesListDbContext yglDbContext)
    {
        _logger = logger;
        _yglDbContext = yglDbContext;
    }

    public async Task<bool> IsValid(ClaimsPrincipal principal)
    {
        var rawUserId = principal.FindFirst(JwtCustomClaimNames.UserId)?.Value;
        if (!Guid.TryParse(rawUserId, out var userId) || userId == Guid.Empty)
        {
            _logger.LogInformation("Token has missing or invalid {ClaimName} claim.", JwtCustomClaimNames.UserId);
            return false;
        }

        var tokenStamp = principal.FindFirst(JwtCustomClaimNames.SecurityStamp)?.Value;
        if (string.IsNullOrWhiteSpace(tokenStamp))
        {
            _logger.LogInformation("Token has missing {ClaimName} claim.", JwtCustomClaimNames.SecurityStamp);
            return false;
        }

        var currentStamp = await _yglDbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.SecurityStamp)
            .FirstOrDefaultAsync();

        if (currentStamp == null)
        {
            _logger.LogInformation("User {UserId} from token no longer exists.", userId);
            return false;
        }

        if (!string.Equals(currentStamp, tokenStamp, StringComparison.Ordinal))
        {
            _logger.LogInformation("Token security stamp is stale for user {UserId}.", userId);
            return false;
        }

        return true;
    }
}
