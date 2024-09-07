using System.Runtime.InteropServices.JavaScript;

namespace VDT.Lock;

public static partial class TestClass {
    [JSExport]
    public static bool Test() => true;
}
