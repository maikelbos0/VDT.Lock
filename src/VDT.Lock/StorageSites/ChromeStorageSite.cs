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
        return new SecureBuffer(await JSChromeDataStore.Load());
    }
#else
    protected override Task<SecureBuffer?> ExecuteLoad() {
        return Task.FromResult<SecureBuffer?>(null);
    }
#endif


#if BROWSER
    // TODO refactor ExecuteSave to accept SecureBuffer
    protected override Task<bool> ExecuteSave(ReadOnlySpan<byte> encryptedSpan) {
        // We're not disposing because we need it async; this is temporary
        var encryptedBuffer = new SecureBuffer(encryptedSpan.ToArray());

        return ExecuteSaveTemp(encryptedBuffer);
    }
    
    private async Task<bool> ExecuteSaveTemp(SecureBuffer encryptedBuffer) {
        
        await JSChromeDataStore.Save(encryptedBuffer.Value);
        return true;
    }
#else
    protected override Task<bool> ExecuteSave(ReadOnlySpan<byte> encryptedSpan) {
        return Task.FromResult(false);
    }
#endif
}
