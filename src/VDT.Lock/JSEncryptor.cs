#if BROWSER
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

namespace VDT.Lock;

public static partial class JSEncryptor {
    // TODO this could be encapsulated better by having external and internal methods
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
}
#endif
