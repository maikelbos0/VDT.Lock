#if BROWSER
using System.Runtime.InteropServices.JavaScript;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using VDT.Lock.StorageSites;

namespace VDT.Lock;

public static partial class TestClass {
    [JSExport]
    public static async Task<string> TestEncryption(string value) {
        using var plainBuffer = new SecureBuffer(Encoding.UTF8.GetBytes(value));
        var encryptor = new Encryptor();
        using var key = new SecureBuffer(RandomNumberGenerator.GetBytes(Encryptor.KeySizeInBytes));
        using var encryptedBytes = await encryptor.Encrypt(plainBuffer, key);
        using var resultBuffer = await encryptor.Decrypt(encryptedBytes, key);

        return Encoding.UTF8.GetString(resultBuffer.Value);
    }

    [JSExport]
    public static async Task<string> TestChromeStorage(string value) {
        for (int i = 0; i < 10; i++) {
            value += " " + value;
        }

        using var plainBuffer = new SecureBuffer(Encoding.UTF8.GetBytes(value));
        var chromeStorageSite = new ChromeStorageSite([], new());
        await chromeStorageSite.Save(plainBuffer);
        var storedValue = await chromeStorageSite.Load();

        return Encoding.UTF8.GetString(storedValue?.Value ?? []);
    }
}
#endif
