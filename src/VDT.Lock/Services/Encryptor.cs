using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

#if BROWSER
using VDT.Lock.JavascriptInterop;
#else
using System.IO;
#endif

namespace VDT.Lock.Services;

public sealed class Encryptor : IEncryptor {
    public const int KeySizeInBytes = 32;
    public const int BlockSizeInBytes = 16;

#if BROWSER
    public async Task<SecureBuffer> Encrypt(SecureBuffer plainBuffer, SecureBuffer keyBuffer) {
        await JSEncryptor.ImportModule();

        using var ivBuffer = new SecureBuffer(RandomNumberGenerator.GetBytes(BlockSizeInBytes));
        using var encryptedBuffer = new SecureBuffer(await JSEncryptor.Encrypt(plainBuffer.Value, keyBuffer.Value, ivBuffer.Value));

        var payloadBuffer = new SecureBuffer(ivBuffer.Value.Length + encryptedBuffer.Value.Length);
        Buffer.BlockCopy(ivBuffer.Value, 0, payloadBuffer.Value, 0, ivBuffer.Value.Length);
        Buffer.BlockCopy(encryptedBuffer.Value, 0, payloadBuffer.Value, ivBuffer.Value.Length, encryptedBuffer.Value.Length);

        return payloadBuffer;
    }
#else
    public Task<SecureBuffer> Encrypt(SecureBuffer plainBuffer, SecureBuffer keyBuffer) {
        using var ivBuffer = new SecureBuffer(RandomNumberGenerator.GetBytes(BlockSizeInBytes));
#pragma warning disable CA1416 // Validate platform compatibility
        using var aes = Aes.Create();
#pragma warning restore CA1416 // Validate platform compatibility
        aes.Mode = CipherMode.CBC;

        using var encryptor = aes.CreateEncryptor(keyBuffer.Value, ivBuffer.Value);

        var payloadBuffer = new SecureBuffer(plainBuffer.Value.Length + 2 * BlockSizeInBytes - plainBuffer.Value.Length % BlockSizeInBytes);
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
    public async Task<SecureBuffer> Decrypt(SecureBuffer encryptedBuffer, SecureBuffer keyBuffer) {
        await JSEncryptor.ImportModule();

        using var ivBuffer = new SecureBuffer(BlockSizeInBytes);
        Buffer.BlockCopy(encryptedBuffer.Value, 0, ivBuffer.Value, 0, BlockSizeInBytes);

        using var payloadBuffer = new SecureBuffer(encryptedBuffer.Value.Length - BlockSizeInBytes);
        Buffer.BlockCopy(encryptedBuffer.Value, BlockSizeInBytes, payloadBuffer.Value, 0, encryptedBuffer.Value.Length - BlockSizeInBytes);

        return new SecureBuffer(await JSEncryptor.Decrypt(payloadBuffer.Value, keyBuffer.Value, ivBuffer.Value));
    }
#else
    public Task<SecureBuffer> Decrypt(SecureBuffer encryptedBuffer, SecureBuffer keyBuffer) {
        using var ivBuffer = new SecureBuffer(BlockSizeInBytes);
        Buffer.BlockCopy(encryptedBuffer.Value, 0, ivBuffer.Value, 0, BlockSizeInBytes);

        using var payloadBuffer = new SecureBuffer(encryptedBuffer.Value.Length - BlockSizeInBytes);
        Buffer.BlockCopy(encryptedBuffer.Value, BlockSizeInBytes, payloadBuffer.Value, 0, encryptedBuffer.Value.Length - BlockSizeInBytes);

#pragma warning disable CA1416 // Validate platform compatibility
        using var aes = Aes.Create();
#pragma warning restore CA1416 // Validate platform compatibility
        aes.Mode = CipherMode.CBC;

        using var decryptor = aes.CreateDecryptor(keyBuffer.Value, ivBuffer.Value);
        using var cryptoStream = new CryptoStream(new MemoryStream(payloadBuffer.Value), decryptor, CryptoStreamMode.Read);
        using var plainBytes = new SecureByteList(cryptoStream);

        return Task.FromResult(plainBytes.ToBuffer());
    }
#endif
}
