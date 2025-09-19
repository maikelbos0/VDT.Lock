#if BROWSER
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

namespace VDT.Lock.JavascriptInterop;

public static partial class JSChromeDataStore {
    public static Task ImportModule() => JSHost.ImportAsync("ChromeDataStore", "../chrome-data-store.js");

    public static async Task<byte[]?> Load(string name) {
        await ImportModule();

        return await LoadInternal(name) as byte[];
    }

    [JSImport("Load", "ChromeDataStore")]
    [return: JSMarshalAs<JSType.Promise<JSType.Any>>()]
    public static partial Task<object?> LoadInternal(string name);

    public static async Task<bool> Clear() {
        await ImportModule();

        return await ClearInternal();
    }

    [JSImport("Clear", "ChromeDataStore")]
    [return: JSMarshalAs<JSType.Promise<JSType.Boolean>>()]
    private static partial Task<bool> ClearInternal();

    public static async Task<bool> Save(string name, byte[] bytes) {
        await ImportModule();

        return await SaveInternal(name, bytes);
    }

    [JSImport("Save", "ChromeDataStore")]
    [return: JSMarshalAs<JSType.Promise<JSType.Boolean>>()]
    private static partial Task<bool> SaveInternal(string name, byte[] bytes);
}
#endif
