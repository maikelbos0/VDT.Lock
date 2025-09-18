#if BROWSER
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;

namespace VDT.Lock;

public static partial class TestClass {
    [JSExport]
    public static async Task<string> TestEncryption(string password) {
        using var plainBuffer = new SecureBuffer(Encoding.UTF8.GetBytes(password));
        var randomByteGenerator = new RandomByteGenerator();
        var encryptor = new Encryptor(randomByteGenerator);
        using var key = new SecureBuffer(randomByteGenerator.Generate(Encryptor.KeySizeInBytes));
        using var encryptedBytes = await encryptor.Encrypt(plainBuffer, key);
        using var resultBuffer = await encryptor.Decrypt(encryptedBytes, key);

        return Encoding.UTF8.GetString(resultBuffer.Value);
    }
}
#endif
