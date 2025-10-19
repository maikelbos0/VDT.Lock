using System.Security.Cryptography;

namespace VDT.Lock.Api.Services;

public sealed class SecretHasher : ISecretHasher {
    public const int SaltSize = 16;
    public const int Iterations = 1_000_000;

    public (byte[] salt, byte[] hash) HashSecret(byte[] secret) {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(secret, salt, Iterations, HashAlgorithmName.SHA512, SHA512.HashSizeInBytes);

        return (salt, hash);
    }

    public bool VerifySecret(byte[] salt, byte[] secret, byte[] expectedHash) {
        var actualHash = Rfc2898DeriveBytes.Pbkdf2(secret, salt, Iterations, HashAlgorithmName.SHA512, SHA512.HashSizeInBytes);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}
