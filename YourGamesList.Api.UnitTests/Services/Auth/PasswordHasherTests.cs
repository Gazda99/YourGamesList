using System;
using System.Security.Cryptography;
using AutoFixture;
using YourGamesList.Api.Services.Auth;

namespace YourGamesList.Api.UnitTests.Services.Auth;

public class PasswordHasherTests
{
    private const int Iterations = 10000;
    private const int SaltSizeBytes = 16;
    private const int HashSizeBytes = 32;

    private IFixture _fixture;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
    }

    [Test]
    public void HashPassword_HashesPassword()
    {
        //ARRANGE
        var password = _fixture.Create<string>();
        var salt = RandomNumberGenerator.GetBytes(SaltSizeBytes);
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(HashSizeBytes);
        var hashString = Convert.ToBase64String(hash);
        var passwordHasher = new PasswordHasher();

        //ACT
        var res = passwordHasher.HashPassword(password, salt);

        //ASSERT
        Assert.That(res.Salt, Is.EquivalentTo(salt));
        Assert.That(res.HashString, Is.EquivalentTo(hashString));
    }
}