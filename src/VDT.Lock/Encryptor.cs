#if !BROWSER
using System.Security.Cryptography;
#endif

namespace VDT.Lock;

public sealed class Encryptor(IRandomByteGenerator randomByteGenerator) : IEncryptor {
    public const int KeySizeInBytes = 32;
    public const int BlockSizeInBytes = 16;

#if BROWSER
    public async Task<SecureBuffer> Encrypt(SecureBuffer plainBuffer, SecureBuffer keyBuffer) {
        await JSEncryptor.ImportModule();

        using var ivBuffer = new SecureBuffer(randomByteGenerator.Generate(BlockSizeInBytes));
        using var encryptedBuffer = new SecureBuffer(await JSEncryptor.Encrypt(plainBuffer.Value, keyBuffer.Value, ivBuffer.Value) as byte[] ?? throw new InvalidOperationException());

        var payloadBuffer = new SecureBuffer(new byte[ivBuffer.Value.Length + encryptedBuffer.Value.Length]);
        Buffer.BlockCopy(ivBuffer.Value, 0, payloadBuffer.Value, 0, ivBuffer.Value.Length);
        Buffer.BlockCopy(encryptedBuffer.Value, 0, payloadBuffer.Value, ivBuffer.Value.Length, encryptedBuffer.Value.Length);

        return payloadBuffer;
    }
#else
    public Task<SecureBuffer> Encrypt(SecureBuffer plainBuffer, SecureBuffer keyBuffer) {
        using var ivBuffer = new SecureBuffer(randomByteGenerator.Generate(BlockSizeInBytes));
#pragma warning disable CA1416 // Validate platform compatibility
        using var aes = Aes.Create();
#pragma warning restore CA1416 // Validate platform compatibility
        aes.Mode = CipherMode.CBC;

        using var encryptor = aes.CreateEncryptor(keyBuffer.Value, ivBuffer.Value);

        var payloadBuffer = new SecureBuffer(new byte[plainBuffer.Value.Length + 2 * BlockSizeInBytes - (plainBuffer.Value.Length % BlockSizeInBytes)]);
        Buffer.BlockCopy(ivBuffer.Value, 0, payloadBuffer.Value, 0, BlockSizeInBytes);

        using var payloadStream = new MemoryStream(payloadBuffer.Value);
        payloadStream.Position = BlockSizeInBytes;

        using var cryptoStream = new CryptoStream(payloadStream, encryptor, CryptoStreamMode.Write);

        cryptoStream.Write(plainBuffer.Value, 0, plainBuffer.Value.Length);
        cryptoStream.FlushFinalBlock();

        return Task.FromResult(payloadBuffer);
    }
#endif

#if BROWSER
    public async Task<SecureBuffer> Decrypt(SecureBuffer payloadBuffer, SecureBuffer keyBuffer) {
        await JSEncryptor.ImportModule();

        using var ivBuffer = new SecureBuffer(new byte[BlockSizeInBytes]);
        Buffer.BlockCopy(payloadBuffer.Value, 0, ivBuffer.Value, 0, BlockSizeInBytes);

        using var encryptedBuffer = new SecureBuffer(new byte[payloadBuffer.Value.Length - BlockSizeInBytes]);
        Buffer.BlockCopy(payloadBuffer.Value, BlockSizeInBytes, encryptedBuffer.Value, 0, payloadBuffer.Value.Length - BlockSizeInBytes);

        return new SecureBuffer(await JSEncryptor.Decrypt(encryptedBuffer.Value, keyBuffer.Value, ivBuffer.Value) as byte[] ?? throw new InvalidOperationException());
    }
#else
    public Task<SecureBuffer> Decrypt(SecureBuffer payloadBuffer, SecureBuffer keyBuffer) {
        using var ivBuffer = new SecureBuffer(new byte[BlockSizeInBytes]);
        Buffer.BlockCopy(payloadBuffer.Value, 0, ivBuffer.Value, 0, BlockSizeInBytes);

        using var encryptedBuffer = new SecureBuffer(new byte[payloadBuffer.Value.Length - BlockSizeInBytes]);
        Buffer.BlockCopy(payloadBuffer.Value, BlockSizeInBytes, encryptedBuffer.Value, 0, payloadBuffer.Value.Length - BlockSizeInBytes);

#pragma warning disable CA1416 // Validate platform compatibility
        using var aes = Aes.Create();
#pragma warning restore CA1416 // Validate platform compatibility
        aes.Mode = CipherMode.CBC;

        using var decryptor = aes.CreateDecryptor(keyBuffer.Value, ivBuffer.Value);
        using var cryptoStream = new CryptoStream(new MemoryStream(encryptedBuffer.Value), decryptor, CryptoStreamMode.Read);
        using var plainBytes = new SecureByteList(cryptoStream);

        return Task.FromResult(plainBytes.ToBuffer());
    }
#endif
}
