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
        // TODO null check
        return new SecureBuffer(await JSChromeDataStore.Load());
    }
#else
    protected override Task<SecureBuffer?> ExecuteLoad() {
        return Task.FromResult<SecureBuffer?>(null);
    }
#endif


#if BROWSER
    protected async override Task<bool> ExecuteSave(SecureBuffer encryptedBuffer) {
        // TODO return actual result
        await JSChromeDataStore.Save(encryptedBuffer.Value);

        return true;
    }
#else
    protected override Task<bool> ExecuteSave(SecureBuffer encryptedBuffer) {
        return Task.FromResult(false);
    }
#endif
}
