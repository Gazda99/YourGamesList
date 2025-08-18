using System;
using System.Security.Cryptography;
using YourGamesList.Api.Services.Auth.Model;

namespace YourGamesList.Api.Services.Auth;

public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a given password using a randomly generated salt.
    /// </summary>
    /// <returns>A <see cref="HashedPassword"/> object containing the hash and the generated salt</returns>
    HashedPassword HashPassword(string password);

    /// <summary>
    /// Hashes a given password using a provided salt.
    /// </summary>
    /// <returns>A <see cref="HashedPassword"/> object containing the hash and the provided salt</returns>
    HashedPassword HashPassword(string password, byte[] salt);
}

public class PasswordHasher : IPasswordHasher
{
    private const int Iterations = 10000;
    private const int SaltSizeBytes = 16;
    private const int HashSizeBytes = 32;

    public HashedPassword HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSizeBytes);
        return HashPassword(password, salt);
    }

    public HashedPassword HashPassword(string password, byte[] salt)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(HashSizeBytes);
        var hashString = Convert.ToBase64String(hash);

        return new HashedPassword(hashString, salt);
    }
}