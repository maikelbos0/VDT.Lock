#if BROWSER
using System.Runtime.InteropServices.JavaScript;
#else
using System.Security.Cryptography;
#endif

namespace VDT.Lock;

public sealed partial class Encryptor {
    public const int KeySizeInBytes = 32;
    public const int BlockSizeInBytes = 16;

    private readonly IRandomByteGenerator randomByteGenerator;

    public Encryptor(IRandomByteGenerator randomByteGenerator) {
        this.randomByteGenerator = randomByteGenerator;
    }

#if BROWSER
    public async Task<byte[]> Encrypt(Stream plainStream, byte[] key) {
        await ImportModule();

        var iv = randomByteGenerator.Generate(BlockSizeInBytes);
        var memoryStream = new MemoryStream();
        await plainStream.CopyToAsync(memoryStream);

        var plainBytes = memoryStream.ToArray();

        var encryptedBytes = await Encrypt(plainBytes, key, iv) as byte[] ?? throw new InvalidOperationException();
        
        var result = new byte[iv.Length +  encryptedBytes.Length];
        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
        Buffer.BlockCopy(encryptedBytes, 0, result, iv.Length, encryptedBytes.Length);
        return result;
    }
#else
    public Task<byte[]> Encrypt(Stream plainStream, byte[] key) {
        var iv = randomByteGenerator.Generate(BlockSizeInBytes);
#pragma warning disable CA1416 // Validate platform compatibility
        using var aes = Aes.Create();
#pragma warning restore CA1416 // Validate platform compatibility
        aes.Mode = CipherMode.CBC;

        var encryptor = aes.CreateEncryptor(key, iv);
        var encryptedStream = new MemoryStream();
        encryptedStream.Write(iv, 0, iv.Length);

        using var cryptoStream = new CryptoStream(encryptedStream, encryptor, CryptoStreamMode.Write);

        plainStream.CopyTo(cryptoStream);
        cryptoStream.FlushFinalBlock();

        return Task.FromResult(encryptedStream.ToArray());
    }
#endif

#if BROWSER
    public async Task<Stream> Decrypt(byte[] encryptedBytes, byte[] key) {
        await ImportModule();

        var iv = encryptedBytes.Take(BlockSizeInBytes).ToArray();

        encryptedBytes = encryptedBytes.Skip(BlockSizeInBytes).ToArray();

        var plainBytes = await Decrypt(encryptedBytes, key, iv) as byte[] ?? throw new InvalidOperationException();

        var plainStream = new MemoryStream();
        plainStream.Write(plainBytes, 0, plainBytes.Length);
        plainStream.Seek(0, SeekOrigin.Begin);

        return plainStream;
    }
#else
    public Task<Stream> Decrypt(byte[] encryptedBytes, byte[] key) {
        var iv = encryptedBytes.Take(BlockSizeInBytes).ToArray();

        encryptedBytes = encryptedBytes.Skip(BlockSizeInBytes).ToArray();

#pragma warning disable CA1416 // Validate platform compatibility
        using var aes = Aes.Create();
#pragma warning restore CA1416 // Validate platform compatibility
        aes.Mode = CipherMode.CBC;

        var decryptor = aes.CreateDecryptor(key, iv);

        return Task.FromResult((Stream)new CryptoStream(new MemoryStream(encryptedBytes), decryptor, CryptoStreamMode.Read));
    }
#endif

#if BROWSER
    public static Task ImportModule() => JSHost.ImportAsync("Encryptor", "../encryptor.js");

    [JSImport("Encrypt", "Encryptor")]
    [return: JSMarshalAs<JSType.Promise<JSType.Any>>()]
    public static partial Task<object?> Encrypt(
        [JSMarshalAs<JSType.Array<JSType.Number>>] byte[] plainBytes,
        [JSMarshalAs<JSType.Array<JSType.Number>>] byte[] key,
        [JSMarshalAs<JSType.Array<JSType.Number>>] byte[] iv
    );

    [JSImport("Decrypt", "Encryptor")]
    [return: JSMarshalAs<JSType.Promise<JSType.Any>>()]
    public static partial Task<object?> Decrypt(
        [JSMarshalAs<JSType.Array<JSType.Number>>] byte[] encryptedBytes,
        [JSMarshalAs<JSType.Array<JSType.Number>>] byte[] key,
        [JSMarshalAs<JSType.Array<JSType.Number>>] byte[] iv
    );
#endif
}
