#if BROWSER
using System.Runtime.InteropServices.JavaScript;
using System.Text;

namespace VDT.Lock;

public static partial class TestClass {
    [JSExport]
    public static async Task<string> Test() {
        using var plainStream = new MemoryStream();
        plainStream.Write("password"u8);
        plainStream.Seek(0, SeekOrigin.Begin);

        var randomByteGenerator = new RandomByteGenerator();
        var encryptor = new Encryptor(randomByteGenerator);
        var key = randomByteGenerator.Generate(Encryptor.KeySizeInBytes);
        var encryptedBytes = await encryptor.Encrypt(plainStream, key);
        var result = await encryptor.Decrypt(encryptedBytes, key);

        var resultStream = new MemoryStream();
        result.CopyTo(resultStream);

        return Encoding.UTF8.GetString(resultStream.ToArray());
    }
}
#endif
