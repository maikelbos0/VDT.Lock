using System;
using System.Threading.Tasks;

#if BROWSER
using VDT.Lock.JavascriptInterop;
#endif

namespace VDT.Lock.StorageSites;

public partial class ChromeStorageSite : StorageSiteBase {
    public ChromeStorageSite(ReadOnlySpan<byte> plainNameSpan, StorageSettings storageSettings) : base(plainNameSpan, storageSettings) { }

#if BROWSER
    protected override async Task<SecureBuffer?> ExecuteLoad() {
        var result = await JSChromeDataStore.Load();

        if (result != null) {
            return new SecureBuffer(result);
        }
        else {
            return null;
        }
    }
#else
    protected override Task<SecureBuffer?> ExecuteLoad()
        => Task.FromResult<SecureBuffer?>(null);
#endif


#if BROWSER
    protected override Task<bool> ExecuteSave(SecureBuffer encryptedBuffer)
        => JSChromeDataStore.Save(encryptedBuffer.Value);
#else
    protected override Task<bool> ExecuteSave(SecureBuffer encryptedBuffer)
        => Task.FromResult(false);
#endif
}
