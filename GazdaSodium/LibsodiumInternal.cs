using System.Runtime.InteropServices;

namespace GazdaSodium;

internal static partial class LibsodiumInternal
{
    private const string LibsodiumDll = "libsodium.dll";

    [DllImport(LibsodiumDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int sodium_init();


    [DllImport(LibsodiumDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int crypto_pwhash_str([Out] byte[] @out, byte[] passwd, ulong passwdlen, ulong opslimit, nuint memlimit);

    [DllImport(LibsodiumDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int crypto_pwhash_str_verify(byte[] str, byte[] passwd, ulong passwdlen);
}