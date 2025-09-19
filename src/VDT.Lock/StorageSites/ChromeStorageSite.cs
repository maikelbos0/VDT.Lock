using System;
using System.Threading.Tasks;

#if BROWSER
using VDT.Lock.JavascriptInterop;
#endif

namespace VDT.Lock.StorageSites;

public partial class ChromeStorageSite : StorageSiteBase {
    private const int sectionSize = 1024 * 5;
    private const string headerKey = "Header";
    private const string sectionKey = "Section";

    public ChromeStorageSite(ReadOnlySpan<byte> plainNameSpan, StorageSettings storageSettings) : base(plainNameSpan, storageSettings) { }

#if BROWSER
    protected override async Task<SecureBuffer?> ExecuteLoad() {
        byte[]? result = null; //await JSChromeDataStore.Load();

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
    protected override async Task<bool> ExecuteSave(SecureBuffer encryptedBuffer) {
        var encryptedBufferSplitter = new SecureBufferSplitter(encryptedBuffer, sectionSize);
        var result = await JSChromeDataStore.Clear();

        if (!result) {
            return result;
        }

        result &= await JSChromeDataStore.Save(headerKey, encryptedBufferSplitter.GetHeader());

        for (var sectionIndex = 0; sectionIndex < encryptedBufferSplitter.SectionCount; sectionIndex++) {
            using var sectionBuffer = encryptedBufferSplitter.GetSectionBuffer(sectionIndex);

            result &= await JSChromeDataStore.Save($"{sectionKey}{sectionIndex}", sectionBuffer.Value);
        }

        return result;
    }
#else
    protected override Task<bool> ExecuteSave(SecureBuffer encryptedBuffer)
        => Task.FromResult(false);
#endif
}
