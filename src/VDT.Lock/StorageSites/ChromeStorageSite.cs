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
        var header = await JSChromeDataStore.Load(headerKey);

        if (header == null || header.Length != 4) {
            return null;
        }

        var sectionCount = header[0] | (header[1] << 8) | (header[2] << 16) | (header[3] << 24);
        using var encryptedBytes = new SecureByteList(sectionCount * sectionSize);

        for (var sectionIndex = 0; sectionIndex < sectionCount; sectionIndex++) {
            var sectionBytes = await JSChromeDataStore.Load($"{sectionKey}{sectionIndex}");

            if (sectionBytes == null) {
                return null;
            }

            using var sectionBuffer = new SecureBuffer(sectionBytes);
            encryptedBytes.Add(sectionBuffer.Value);
        }

        return encryptedBytes.ToBuffer();
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
