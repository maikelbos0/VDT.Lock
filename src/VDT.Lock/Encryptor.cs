using System.Security.Cryptography;

namespace VDT.Lock;

public sealed class Encryptor {
    public const int KeySizeInBytes = 32;
    public const int BlockSizeInBytes = 16;

    public byte[] Encrypt(Stream stream, byte[] key) {
        using var aes = Aes.Create();
        var iv = RandomNumberGenerator.GetBytes(BlockSizeInBytes);

        var encryptor = aes.CreateEncryptor(key, iv);
        using var encryptedStream = new MemoryStream();
        using var cryptoStream = new CryptoStream(encryptedStream, encryptor, CryptoStreamMode.Write);

        stream.CopyTo(cryptoStream);

        return encryptedStream.ToArray();
    }

    public Stream Decrypt(byte[] bytes, byte[] key) {
        return null!;
    }
}
