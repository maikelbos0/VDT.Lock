using System.Security.Cryptography;

namespace VDT.Lock.Api.Services;

public sealed class SecretHasher {
    public const int SaltSize = 16;
    public const int Iterations = 1_000_000;

    public (byte[] salt, byte[] hash) HashSecret(byte[] secret) {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(secret, salt, Iterations, HashAlgorithmName.SHA512, SHA512.HashSizeInBytes);
        
        return (salt, hash);
    }
}
