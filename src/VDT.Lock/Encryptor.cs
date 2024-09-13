#if BROWSER
using System.Runtime.InteropServices.JavaScript;
#endif
using System.Security.Cryptography;

namespace VDT.Lock;

public sealed partial class Encryptor {
    public const int KeySizeInBytes = 32;
    public const int BlockSizeInBytes = 16;

    private readonly IRandomByteGenerator randomByteGenerator;

    public Encryptor(IRandomByteGenerator randomByteGenerator) {
        this.randomByteGenerator = randomByteGenerator;
    }

#if BROWSER
    public async Task<byte[]> Encrypt(Stream inputStream, byte[] key) {
        var iv = randomByteGenerator.Generate(BlockSizeInBytes);

        await JSHost.ImportAsync("Encryptor", "../encryptor.js");

        var memoryStream = new MemoryStream();
        await inputStream.CopyToAsync(memoryStream);

        var input = memoryStream.ToArray();

        var encryptedBytes = await Encrypt(input, key, iv) as byte[] ?? throw new InvalidOperationException();
        
        var result = new byte[iv.Length +  encryptedBytes.Length];
        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
        Buffer.BlockCopy(encryptedBytes, 0, result, iv.Length, encryptedBytes.Length);
        return result;
    }
#else
    public Task<byte[]> Encrypt(Stream inputStream, byte[] key) {
        var iv = randomByteGenerator.Generate(BlockSizeInBytes);
#pragma warning disable CA1416 // Validate platform compatibility
        using var aes = Aes.Create();
#pragma warning restore CA1416 // Validate platform compatibility
        aes.Mode = CipherMode.CBC;

        var encryptor = aes.CreateEncryptor(key, iv);
        var encryptedStream = new MemoryStream();
        encryptedStream.Write(iv, 0, iv.Length);

        using var cryptoStream = new CryptoStream(encryptedStream, encryptor, CryptoStreamMode.Write);

        inputStream.CopyTo(cryptoStream);
        cryptoStream.FlushFinalBlock();

        return Task.FromResult(encryptedStream.ToArray());
    }
#endif

    public Stream Decrypt(byte[] encryptedBytes, byte[] key) {
        var iv = encryptedBytes.Take(BlockSizeInBytes).ToArray();

        encryptedBytes = encryptedBytes.Skip(BlockSizeInBytes).ToArray();

#pragma warning disable CA1416 // Validate platform compatibility
        using var aes = Aes.Create();
#pragma warning restore CA1416 // Validate platform compatibility
        aes.Mode = CipherMode.CBC;

        var decryptor = aes.CreateDecryptor(key, iv);

        return new CryptoStream(new MemoryStream(encryptedBytes), decryptor, CryptoStreamMode.Read);
    }

#if BROWSER
    [JSImport("Encrypt", "Encryptor")]
    [return: JSMarshalAs<JSType.Promise<JSType.Any>>()]
    public static partial Task<object?> Encrypt(
        [JSMarshalAs<JSType.Array<JSType.Number>>] byte[] input,
        [JSMarshalAs<JSType.Array<JSType.Number>>] byte[] key,
        [JSMarshalAs<JSType.Array<JSType.Number>>] byte[] iv
    );
#endif
}
