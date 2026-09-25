using System;
using System.Security.Cryptography;
using System.Text;
using SQLitePCL;

namespace YourGamesList.Api.Services.Auth;

//TODO: unit tests
public interface IVerificationTokenHasher
{
    (string RawToken, string TokenHash) GenerateToken();
    string HashToken(string rawToken);
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
            .Replace('/', '_')
            .ToLower();

        var tokenHash = HashToken(rawToken).ToLower();
        ;

        return (rawToken, tokenHash);
    }

    public bool CompareToken(string rawToken, string hashedToken)
    {
        rawToken = rawToken.ToLower();
        hashedToken = hashedToken.ToLower();
        var rawHashed = HashToken(rawToken).ToLower();

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(rawHashed),
            Encoding.UTF8.GetBytes(hashedToken)
        );
    }

    public string HashToken(string rawToken)
    {
        var inputBytes = Encoding.UTF8.GetBytes(rawToken.ToLower());
        var hashBytes = SHA512.HashData(inputBytes);
        return Convert.ToHexString(hashBytes).ToLower();
    }
}