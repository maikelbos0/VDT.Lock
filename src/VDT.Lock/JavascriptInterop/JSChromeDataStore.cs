#if BROWSER
using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

namespace VDT.Lock.JavascriptInterop;

public static partial class JSChromeDataStore {
    public static Task ImportModule() => JSHost.ImportAsync("ChromeDataStore", "../chrome-data-store.js");

    public static async Task<byte[]> Load() {
        await ImportModule();

        return await LoadInternal() as byte[] ?? throw new InvalidOperationException();
    }

    [JSImport("Load", "ChromeDataStore")]
    [return: JSMarshalAs<JSType.Promise<JSType.Any>>()]
    public static partial Task<object?> LoadInternal();

    public static async Task Save(byte[] encryptedBytes) {
        await ImportModule();

        await SaveInternal(encryptedBytes);
    }

    [JSImport("Save", "ChromeDataStore")]
    private static partial Task SaveInternal(byte[] encryptedBytes);
}
#endif
