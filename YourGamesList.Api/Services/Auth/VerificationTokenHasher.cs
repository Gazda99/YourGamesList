using System;
using System.Security.Cryptography;
using System.Text;

namespace YourGamesList.Api.Services.Auth;

public interface IVerificationTokenHasher
{
    (string RawToken, string TokenHash) GenerateToken();
    bool CompareToken(string rawToken, string tokenHash);
}

public class VerificationTokenHasher : IVerificationTokenHasher
{
    public (string RawToken, string TokenHash) GenerateToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(32);

        var rawToken = Convert.ToBase64String(randomBytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

        var tokenHash = HashToken(rawToken);

        return (rawToken, tokenHash);
    }

    public bool CompareToken(string rawToken, string hashedToken)
    {
        var rawHashed = HashToken(rawToken);

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(rawHashed),
            Encoding.UTF8.GetBytes(hashedToken)
        );
    }

    private static string HashToken(string rawToken)
    {
        var inputBytes = Encoding.UTF8.GetBytes(rawToken);
        var hashBytes = SHA512.HashData(inputBytes);
        return Convert.ToHexString(hashBytes);
    }
}