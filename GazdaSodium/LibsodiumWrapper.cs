using System;
using System.Text;

namespace GazdaSodium;

/// <summary>
/// Wrapper around Libsodium C library
/// </summary>
public static partial class LibsodiumWrapper
{
    /// <summary>
    /// Underlying Libsodium function sodium_init() returns 0 on success, -1 on failure, and 1 if the library had already been initialized.
    /// </summary>
    public static int SodiumInit()
    {
        return LibsodiumInternal.sodium_init();
    }

    /// <param name="password">Password string to be hashed</param>
    /// <param name="opsLimit">Must be between <see cref="LibsodiumWrapper.CryptoPwHashOpsLimitMin"/> and <see cref="LibsodiumWrapper.CryptoPwHashOpsLimitMax"/></param>
    /// <param name="memLimit">Must be between <see cref="LibsodiumWrapper.CryptoPwHashMemLimitMin"/> and <see cref="LibsodiumWrapper.CryptoPwHashMemLimitMax"/></param>
    /// <exception cref="InvalidOperationException">In case of underlying Libsodium crypto_pwhash_str() call returning status other than 0</exception>
    /// <returns>
    /// ASCII encoded string of max size <see cref="LibsodiumWrapper.CryptoPwhashStrBytes"/>, which includes:
    ///  - the result of a memory-hard, CPU-intensive hash function applied to the <paramref name="password"/>
    ///  - the automatically generated salt used for the previous computation;
    ///  - the other parameters required to verify the password, including the algorithm identifier, its version, <paramref name="opsLimit"/>, and <paramref name="memLimit"/>
    /// </returns>
    public static string CryptoPwhashStr(string password, ulong opsLimit, nuint memLimit)
    {
        var passwordByteCount = Encoding.UTF8.GetByteCount(password);
        var passwordBytes = new byte[passwordByteCount];
        try
        {
            Encoding.UTF8.GetBytes(password, passwordBytes);
            var hashStringSize = LibsodiumInternal.crypto_pwhash_strbytes();
            var hashedResult = new byte[hashStringSize];

            var res = LibsodiumInternal.crypto_pwhash_str(hashedResult, passwordBytes, (ulong) passwordByteCount, opsLimit, memLimit);

            if (res != 0)
            {
                throw new InvalidOperationException($"Libsodium password hashing failed. Return value: '{res.ToString()}'");
            }

            return Encoding.UTF8.GetString(hashedResult).TrimEnd('\0');
        }
        finally
        {
            //Clear password bytes from memory
            Array.Clear(passwordBytes, 0, passwordBytes.Length);
        }
    }

    /// <summary>
    /// Uses underlying Libsodium crypto_pwhash_str_verify() method to check if <paramref name="password"/> matches hash <paramref name="hashedPassword"/>
    /// </summary>
    /// <returns>True if password matches hash, else False</returns>
    public static bool CryptoPwhashStrVerify(string hashedPassword, string password)
    {
        var hashByteCount = Encoding.UTF8.GetByteCount(hashedPassword);
        var passByteCount = Encoding.UTF8.GetByteCount(password);
        // +1 for zero terminator required by Libsodium
        var hashBytes = new byte[hashByteCount + 1];
        var passwordBytes = new byte[passByteCount];

        try
        {
            Encoding.UTF8.GetBytes(hashedPassword, hashBytes);
            Encoding.UTF8.GetBytes(password, passwordBytes);

            var res = LibsodiumInternal.crypto_pwhash_str_verify(hashBytes, passwordBytes, (ulong) passByteCount);

            return res == 0;
        }
        finally
        {
            //Clear password bytes from memory
            Array.Clear(hashBytes, 0, hashBytes.Length);
            Array.Clear(passwordBytes, 0, passwordBytes.Length);
        }
    }
}