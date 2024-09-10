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

    public async Task<byte[]> Encrypt(Stream stream, byte[] key) {
        var iv = randomByteGenerator.Generate(BlockSizeInBytes);

#if BROWSER
        await JSHost.ImportAsync("Encryptor", "../encryptor.js");

        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);

        var input = memoryStream.ToArray();

        return await Encrypt(input, key, iv) as byte[] ?? throw new InvalidOperationException();
#else
        using var aes = Aes.Create();
        aes.Mode = CipherMode.CBC;

        var encryptor = aes.CreateEncryptor(key, iv);
        using var encryptedStream = new MemoryStream();
        using var cryptoStream = new CryptoStream(encryptedStream, encryptor, CryptoStreamMode.Write);

        stream.CopyTo(cryptoStream);
        cryptoStream.FlushFinalBlock();

        return encryptedStream.ToArray();
#endif
    }

    public Stream Decrypt(byte[] bytes, byte[] key) {
        return null!;
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
