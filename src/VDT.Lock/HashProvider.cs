using System.Security.Cryptography;

namespace VDT.Lock;

public sealed class HashProvider : IHashProvider {
    public const int Iterations = 1_000_000;

    public byte[] Provide(Stream stream, byte[] salt) {
        using var sha = SHA256.Create();

        var password = sha.ComputeHash(stream);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);

        return pbkdf2.GetBytes(SHA256.HashSizeInBytes);
    }
}
