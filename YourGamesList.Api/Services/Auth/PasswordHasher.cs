using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YourGamesList.Api.Services.Auth.Options;

namespace YourGamesList.Api.Services.Auth;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyHashedPassword(string hashedPassword, string password);
}

public class LibSodiumPasswordHasher : IPasswordHasher
{
    private readonly ILogger<LibSodiumPasswordHasher> _logger;
    private readonly IOptions<LibSodiumPasswordHashingOptions> _options;

    public LibSodiumPasswordHasher(ILogger<LibSodiumPasswordHasher> logger, IOptions<LibSodiumPasswordHashingOptions> options, ISodiumInitializer sodiumInitializer)
    {
        _logger = logger;
        _options = options;

        sodiumInitializer.EnsureInitialized();
    }

    public string HashPassword(string password)
    {
        try
        {
            var hashedPassword = LibGazdium.LibsodiumWrapper.CryptoPwhashStr(password, _options.Value.OpsLimit, _options.Value.MemoryLimit);
            if (string.IsNullOrEmpty(hashedPassword))
            {
                _logger.LogWarning("LibGazdium returned an empty hashed password!");
            }

            return hashedPassword;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex.Message);
            return string.Empty;
        }
    }

    public bool VerifyHashedPassword(string hashedPassword, string password)
    {
        var result = LibGazdium.LibsodiumWrapper.CryptoPwhashStrVerify(hashedPassword, password);
        if (result)
        {
            _logger.LogInformation("Password has been verified.");
        }
        else
        {
            _logger.LogInformation("Password verification failed.");
        }

        return result;
    }
}