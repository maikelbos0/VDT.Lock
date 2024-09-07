using System.Runtime.InteropServices.JavaScript;

namespace VDT.Lock;

public static partial class TestClass {
    [JSExport]
    public static string Test() => "Hello world!";
}
