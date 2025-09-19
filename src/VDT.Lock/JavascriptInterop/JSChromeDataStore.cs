#if BROWSER
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

namespace VDT.Lock.JavascriptInterop;

public static partial class JSChromeDataStore {
    public static Task ImportModule() => JSHost.ImportAsync("ChromeDataStore", "../chrome-data-store.js");

    public static async Task<byte[]?> Load() {
        await ImportModule();

        return await LoadInternal() as byte[];
    }

    [JSImport("Load", "ChromeDataStore")]
    [return: JSMarshalAs<JSType.Promise<JSType.Any>>()]
    public static partial Task<object?> LoadInternal();

    public static async Task<bool> Save(byte[] encryptedBytes) {
        await ImportModule();

        return await SaveInternal(encryptedBytes);
    }

    [JSImport("Save", "ChromeDataStore")]
    [return: JSMarshalAs<JSType.Promise<JSType.Boolean>>()]
    private static partial Task<bool> SaveInternal(byte[] encryptedBytes);
}
#endif
