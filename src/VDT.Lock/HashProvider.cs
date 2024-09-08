using System.Security.Cryptography;

namespace VDT.Lock;

public sealed class HashProvider {
    private const int iterations = 1_000_000;

    public byte[] Provide(Stream stream, byte[] salt) {
        using var sha = SHA256.Create();

        var password = sha.ComputeHash(stream);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);

        return pbkdf2.GetBytes(SHA256.HashSizeInBytes);
    }
}
