using System.Runtime.InteropServices.JavaScript;

namespace VDT.Lock;

#if BROWSER
public static partial class JsEncryptor {
    [JSImport("Test", "Encryptor")]
    // For async?
    // [return: JSMarshalAs<JSType.Promise<JSType.String>>()]
    public static partial string Test(string input);
}
#endif
