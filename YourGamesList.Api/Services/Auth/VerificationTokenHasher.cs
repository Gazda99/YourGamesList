using System;
using System.Security.Cryptography;
using System.Text;

namespace YourGamesList.Api.Services.Auth;

public interface IVerificationTokenHasher
{
    /// <summary>
    /// Generates a verification token and its hash.
    /// </summary>
    (string RawToken, string TokenHash) GenerateToken();

    /// <summary>
    /// Hashes a verification token.
    /// </summary>
    string HashToken(string rawToken);
}

public class VerificationTokenHasher : IVerificationTokenHasher
{
    public (string RawToken, string TokenHash) GenerateToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(32);

        var rawToken = Convert.ToBase64String(randomBytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_')
            .ToLower();

        var tokenHash = HashToken(rawToken).ToLower();

        return (rawToken, tokenHash);
    }

    public string HashToken(string rawToken)
    {
        var inputBytes = Encoding.UTF8.GetBytes(rawToken.ToLower());
        var hashBytes = SHA512.HashData(inputBytes);
        return Convert.ToHexString(hashBytes).ToLower();
    }
}