using System;
using System.Runtime.InteropServices;

namespace GazdaSodium;

internal static partial class LibsodiumInternal
{
    [DllImport(LibsodiumDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int crypto_pwhash_alg_argon2i13();

    [DllImport(LibsodiumDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int crypto_pwhash_alg_argon2id13();

    [DllImport(LibsodiumDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int crypto_pwhash_alg_default();

    [DllImport(LibsodiumDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern ulong crypto_pwhash_bytes_max();

    [DllImport(LibsodiumDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern ulong crypto_pwhash_bytes_min();

    [DllImport(LibsodiumDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern ulong crypto_pwhash_memlimit_interactive();

    [DllImport(LibsodiumDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern ulong crypto_pwhash_memlimit_max();

    [DllImport(LibsodiumDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern ulong crypto_pwhash_memlimit_min();

    [DllImport(LibsodiumDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern ulong crypto_pwhash_memlimit_moderate();

    [DllImport(LibsodiumDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern ulong crypto_pwhash_memlimit_sensitive();

    [DllImport(LibsodiumDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern ulong crypto_pwhash_opslimit_interactive();

    [DllImport(LibsodiumDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern ulong crypto_pwhash_opslimit_max();

    [DllImport(LibsodiumDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern ulong crypto_pwhash_opslimit_min();

    [DllImport(LibsodiumDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern ulong crypto_pwhash_opslimit_moderate();

    [DllImport(LibsodiumDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern ulong crypto_pwhash_opslimit_sensitive();

    [DllImport(LibsodiumDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern ulong crypto_pwhash_passwd_max();

    [DllImport(LibsodiumDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern ulong crypto_pwhash_passwd_min();

    [DllImport(LibsodiumDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern ulong crypto_pwhash_saltbytes();

    [DllImport(LibsodiumDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern ulong crypto_pwhash_strbytes();

    [DllImport(LibsodiumDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr crypto_pwhash_strprefix();
}