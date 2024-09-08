using System.Runtime.InteropServices.JavaScript;

namespace VDT.Lock;

#if BROWSER
public static partial class JsEncryptor {
    [JSImport("Test", "Encryptor")]
    [return: JSMarshalAs<JSType.Promise<JSType.String>>()]
    public static partial Task<string> Test(string input, byte[] key);
}
#endif
