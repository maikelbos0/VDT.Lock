#if BROWSER
using System.Runtime.InteropServices.JavaScript;
using System.Text;

namespace VDT.Lock;

public static partial class TestClass {
    [JSExport]
    public static async Task<string> Test() {
        using var plainBuffer = new SecureBuffer("password"u8.ToArray());

        var randomByteGenerator = new RandomByteGenerator();
        var encryptor = new Encryptor(randomByteGenerator);
        var key = randomByteGenerator.Generate(Encryptor.KeySizeInBytes);
        var encryptedBytes = await encryptor.Encrypt(plainBuffer, key);
        var resultBuffer = await encryptor.Decrypt(encryptedBytes, key);

        return Encoding.UTF8.GetString(resultBuffer.Value);
    }
}
#endif
