#if !BROWSER
using System.Security.Cryptography;
#endif

namespace VDT.Lock;

public sealed class Encryptor {
    public const int KeySizeInBytes = 32;
    public const int BlockSizeInBytes = 16;

    private readonly IRandomByteGenerator randomByteGenerator;

    public Encryptor(IRandomByteGenerator randomByteGenerator) {
        this.randomByteGenerator = randomByteGenerator;
    }

#if BROWSER
    public async Task<byte[]> Encrypt(Stream plainStream, byte[] key) {
        await JSEncryptor.ImportModule();

        var iv = randomByteGenerator.Generate(BlockSizeInBytes);
        var memoryStream = new MemoryStream();
        await plainStream.CopyToAsync(memoryStream);

        var plainBytes = memoryStream.ToArray();

        var encryptedBytes = await JSEncryptor.Encrypt(plainBytes, key, iv) as byte[] ?? throw new InvalidOperationException();
        
        var result = new byte[iv.Length +  encryptedBytes.Length];
        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
        Buffer.BlockCopy(encryptedBytes, 0, result, iv.Length, encryptedBytes.Length);
        return result;
    }
#else
    public Task<SecureBuffer> Encrypt(SecureBuffer plainBuffer, byte[] key) {
        // TODO secure this
        var iv = randomByteGenerator.Generate(BlockSizeInBytes);
#pragma warning disable CA1416 // Validate platform compatibility
        using var aes = Aes.Create();
#pragma warning restore CA1416 // Validate platform compatibility
        aes.Mode = CipherMode.CBC;

        var encryptor = aes.CreateEncryptor(key, iv);
        // TODO eliminate memorystream
        var encryptedStream = new MemoryStream();
        encryptedStream.Write(iv, 0, iv.Length);

        using var cryptoStream = new CryptoStream(encryptedStream, encryptor, CryptoStreamMode.Write);

        cryptoStream.Write(plainBuffer.Value, 0, plainBuffer.Value.Length);
        cryptoStream.FlushFinalBlock();
        encryptedStream.Seek(0, SeekOrigin.Begin);

        return Task.FromResult(new SecureBuffer(encryptedStream.ToArray()));
    }
#endif

#if BROWSER
    public async Task<Stream> Decrypt(byte[] encryptedBytes, byte[] key) {
        await JSEncryptor.ImportModule();

        var iv = encryptedBytes.Take(BlockSizeInBytes).ToArray();

        encryptedBytes = encryptedBytes.Skip(BlockSizeInBytes).ToArray();

        var plainBytes = await JSEncryptor.Decrypt(encryptedBytes, key, iv) as byte[] ?? throw new InvalidOperationException();

        var plainStream = new MemoryStream();
        plainStream.Write(plainBytes, 0, plainBytes.Length);
        plainStream.Seek(0, SeekOrigin.Begin);

        return plainStream;
    }
#else
    public Task<SecureBuffer> Decrypt(SecureBuffer payloadBuffer, byte[] key) {
        // TODO eliminate toarray
        using var ivBuffer = new SecureBuffer(payloadBuffer.Value.Take(BlockSizeInBytes).ToArray());
        using var encryptedBuffer = new SecureBuffer(payloadBuffer.Value.Skip(BlockSizeInBytes).ToArray());

#pragma warning disable CA1416 // Validate platform compatibility
        using var aes = Aes.Create();
#pragma warning restore CA1416 // Validate platform compatibility
        aes.Mode = CipherMode.CBC;

        var decryptor = aes.CreateDecryptor(key, ivBuffer.Value);
        using var cryptoStream = new CryptoStream(new MemoryStream(encryptedBuffer.Value), decryptor, CryptoStreamMode.Read);

        // TODO eliminate memorystream
        var encryptedStream = new MemoryStream();
        cryptoStream.CopyTo(encryptedStream);
        encryptedStream.Seek(0, SeekOrigin.Begin);

        return Task.FromResult(new SecureByteList(encryptedStream).ToBuffer());
    }
#endif
}
