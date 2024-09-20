using System.Security.Cryptography;

namespace VDT.Lock;

public sealed class HashProvider : IHashProvider {
    public const int Iterations = 1_000_000;

    public SecureBuffer Provide(SecureBuffer plainBuffer, byte[] salt)
        => new(Rfc2898DeriveBytes.Pbkdf2(plainBuffer.Value, salt, Iterations, HashAlgorithmName.SHA256, SHA256.HashSizeInBytes));
}
