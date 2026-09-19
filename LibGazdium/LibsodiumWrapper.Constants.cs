namespace LibGazdium;

public static partial class LibsodiumWrapper
{
    public static ulong CryptoPwHashMemLimitMax => LibsodiumInternal.crypto_pwhash_memlimit_max();
    public static ulong CryptoPwHashMemLimitMin => LibsodiumInternal.crypto_pwhash_memlimit_min();
    public static ulong CryptoPwHashOpsLimitMax => LibsodiumInternal.crypto_pwhash_opslimit_max();
    public static ulong CryptoPwHashOpsLimitMin => LibsodiumInternal.crypto_pwhash_opslimit_min();
    public static ulong CryptoPwhashStrBytes => LibsodiumInternal.crypto_pwhash_strbytes();
}