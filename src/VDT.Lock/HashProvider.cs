using System.Security.Cryptography;

namespace VDT.Lock;

public sealed class HashProvider : IHashProvider {
    public const int Iterations = 1_000_000;

    public byte[] Provide(SecureByteArray plainBytes, byte[] salt)
        => Rfc2898DeriveBytes.Pbkdf2(plainBytes.GetValue(), salt.AsSpan(), Iterations, HashAlgorithmName.SHA256, SHA256.HashSizeInBytes);
}
