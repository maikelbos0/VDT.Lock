#if BROWSER
using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

namespace VDT.Lock.JavascriptInterop;

public static partial class JSEncryptor {
    public static Task ImportModule() => JSHost.ImportAsync("Encryptor", "../encryptor.js");

    public static async Task<byte[]> Encrypt(byte[] plainBytes, byte[] key, byte[] iv) {
        await ImportModule();

        return await EncryptInternal(plainBytes, key, iv) as byte[] ?? throw new InvalidOperationException();
    }

    [JSImport("encrypt", "Encryptor")]
    [return: JSMarshalAs<JSType.Promise<JSType.Any>>()]
    private static partial Task<object?> EncryptInternal(byte[] plainBytes, byte[] key, byte[] iv);

    public static async Task<byte[]> Decrypt(byte[] encryptedBytes, byte[] key, byte[] iv) {
        await ImportModule();

        return await DecryptInternal(encryptedBytes, key, iv) as byte[] ?? throw new InvalidOperationException();
    }

    [JSImport("decrypt", "Encryptor")]
    [return: JSMarshalAs<JSType.Promise<JSType.Any>>()]
    private static partial Task<object?> DecryptInternal(byte[] encryptedBytes, byte[] key, byte[] iv);
}
#endif
