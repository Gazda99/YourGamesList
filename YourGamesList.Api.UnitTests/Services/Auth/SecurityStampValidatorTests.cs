using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using YourGamesList.Api.Services.Auth;
using YourGamesList.Database;
using YourGamesList.Database.Entity;
using YourGamesList.TestsUtils;

namespace YourGamesList.Api.UnitTests.Services.Auth;

public class SecurityStampValidatorTests
{
    private const string Stamp = "current-stamp";

    private ILogger<SecurityStampValidator> _logger;
    private YourGamesListDbContext _dbContext;

    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger<SecurityStampValidator>>();
        var optionsBuilder = new DbContextOptionsBuilder<YourGamesListDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString());
        _dbContext = new YourGamesListDbContext(optionsBuilder.Options);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Test]
    public async Task IsValid_StampMatches_ReturnsTrue()
    {
        //ARRANGE
        var user = await AddUser(Stamp);
        var principal = CreatePrincipal(user.Id.ToString(), Stamp);
        var validator = CreateValidator();

        //ACT
        var res = await validator.IsValid(principal);

        //ASSERT
        Assert.That(res, Is.True);
    }

    [Test]
    public async Task IsValid_StampIsStale_ReturnsFalse()
    {
        //ARRANGE
        var user = await AddUser(Stamp);
        var principal = CreatePrincipal(user.Id.ToString(), "old-stamp");
        var validator = CreateValidator();

        //ACT
        var res = await validator.IsValid(principal);

        //ASSERT
        Assert.That(res, Is.False);
        _logger.ReceivedLog(LogLevel.Information, $"Token security stamp is stale for user {user.Id}.");
    }

    [Test]
    public async Task IsValid_StampDiffersOnlyByCase_ReturnsFalse()
    {
        //ARRANGE
        var user = await AddUser("abcdef");
        var principal = CreatePrincipal(user.Id.ToString(), "ABCDEF");
        var validator = CreateValidator();

        //ACT
        var res = await validator.IsValid(principal);

        //ASSERT
        Assert.That(res, Is.False);
    }

    [Test]
    public async Task IsValid_UserDoesNotExist_ReturnsFalse()
    {
        //ARRANGE
        var userId = Guid.NewGuid();
        var principal = CreatePrincipal(userId.ToString(), Stamp);
        var validator = CreateValidator();

        //ACT
        var res = await validator.IsValid(principal);

        //ASSERT
        Assert.That(res, Is.False);
        _logger.ReceivedLog(LogLevel.Information, $"User {userId} from token no longer exists.");
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public async Task IsValid_StampClaimMissing_ReturnsFalse(string? stamp)
    {
        //ARRANGE
        var user = await AddUser(Stamp);
        var principal = CreatePrincipal(user.Id.ToString(), stamp);
        var validator = CreateValidator();

        //ACT
        var res = await validator.IsValid(principal);

        //ASSERT
        Assert.That(res, Is.False);
        _logger.ReceivedLog(LogLevel.Information, $"Token has missing {JwtCustomClaimNames.SecurityStamp} claim.");
    }

    [TestCase(null)]
    [TestCase("not-a-guid")]
    [TestCase("00000000-0000-0000-0000-000000000000")]
    public async Task IsValid_UserIdClaimMissingOrInvalid_ReturnsFalse(string? userId)
    {
        //ARRANGE
        var principal = CreatePrincipal(userId, Stamp);
        var validator = CreateValidator();

        //ACT
        var res = await validator.IsValid(principal);

        //ASSERT
        Assert.That(res, Is.False);
        _logger.ReceivedLog(LogLevel.Information, $"Token has missing or invalid {JwtCustomClaimNames.UserId} claim.");
    }

    private async Task<User> AddUser(string securityStamp)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            NormalizedUsername = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashed_password",
            SecurityStamp = securityStamp,
            CreatedAt = DateTimeOffset.UtcNow,
            DateOfBirth = new DateOnly(2000, 1, 1),
            UserStatus = UserStatus.Active
        };
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();
        return user;
    }

    private static ClaimsPrincipal CreatePrincipal(string? userId, string? stamp)
    {
        var claims = new List<Claim>();
        if (userId != null)
        {
            claims.Add(new Claim(JwtCustomClaimNames.UserId, userId));
        }

        if (stamp != null)
        {
            claims.Add(new Claim(JwtCustomClaimNames.SecurityStamp, stamp));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    private SecurityStampValidator CreateValidator()
    {
        return new SecurityStampValidator(_logger, _dbContext);
    }
}
