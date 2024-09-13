using System.Runtime.InteropServices.JavaScript;
using System.Security.Cryptography;

namespace VDT.Lock;

public sealed partial class Encryptor {
    public const int KeySizeInBytes = 32;
    public const int BlockSizeInBytes = 16;

    private readonly IRandomByteGenerator randomByteGenerator;

    public Encryptor(IRandomByteGenerator randomByteGenerator) {
        this.randomByteGenerator = randomByteGenerator;
    }

    public async Task<byte[]> Encrypt(Stream inputStream, byte[] key) {
        var iv = randomByteGenerator.Generate(BlockSizeInBytes);

#if BROWSER
        await JSHost.ImportAsync("Encryptor", "../encryptor.js");

        var memoryStream = new MemoryStream();
        await inputStream.CopyToAsync(memoryStream);

        var input = memoryStream.ToArray();

        var encryptedBytes = await Encrypt(input, key, iv) as byte[] ?? throw new InvalidOperationException();
#else
        using var aes = Aes.Create();
        aes.Mode = CipherMode.CBC;

        var encryptor = aes.CreateEncryptor(key, iv);
        var encryptedStream = new MemoryStream();
        using var cryptoStream = new CryptoStream(encryptedStream, encryptor, CryptoStreamMode.Write);

        inputStream.CopyTo(cryptoStream);
        cryptoStream.FlushFinalBlock();

        var encryptedBytes = encryptedStream.ToArray();

#endif
        
        var result = new byte[iv.Length +  encryptedBytes.Length];
        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
        Buffer.BlockCopy(encryptedBytes, 0, result, iv.Length, encryptedBytes.Length);
        return result;
    }

    public Stream Decrypt(byte[] bytes, byte[] key) {
        var iv = bytes.Take(BlockSizeInBytes).ToArray();

        bytes = bytes.Skip(BlockSizeInBytes).ToArray();

        using var aes = Aes.Create();
        aes.Mode = CipherMode.CBC;

        var decryptor = aes.CreateDecryptor(key, iv);

        return new CryptoStream(new MemoryStream(bytes), decryptor, CryptoStreamMode.Read);
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

    // TODO rename bytes/encryptedbytes = encryptedData / encryptedBytes?
    // TODO rename stream = inputStream / plainDataStream?
}
