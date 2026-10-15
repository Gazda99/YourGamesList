using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace YourGamesList.Api.Services.Auth;

public interface ISodiumInitializer
{
    /// <summary>
    /// Initializes libsodium if needed.
    /// </summary>
    void EnsureInitialized();
}

[ExcludeFromCodeCoverage]
public class SodiumInitializer : ISodiumInitializer
{
    private static readonly Lock InitializationLock = new();
    private static bool _isInitialized;

    private readonly ILogger<SodiumInitializer> _logger;

    public SodiumInitializer(ILogger<SodiumInitializer> logger)
    {
        _logger = logger;
    }

    public void EnsureInitialized()
    {
        if (_isInitialized)
        {
            return;
        }

        lock (InitializationLock)
        {
            if (!_isInitialized)
            {
                var initializationResult = LibGazdium.LibsodiumWrapper.SodiumInit();
                if (initializationResult == -1)
                {
                    throw new InvalidOperationException("Libsodium init failed!");
                }

                _logger.LogInformation("Libsodium library initialized with code: {LibsodiumInitializationResult}", initializationResult);
                _isInitialized = true;
            }
        }
    }
}