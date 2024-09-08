using System.Runtime.InteropServices.JavaScript;

namespace VDT.Lock;

public static partial class TestClass {
    [JSExport]
    public static string Test() => "Hello world!";

    [JSExport]
    public static string Test2() {
        using var stream = new MemoryStream();
        var password = "password"u8.ToArray();
        stream.Write(password, 0, password.Length);
        stream.Seek(0, SeekOrigin.Begin);

        var encryptor = new Encryptor();
        var result = encryptor.Encrypt(stream, new byte[32]);

        return Convert.ToHexString(result);
    }
}
