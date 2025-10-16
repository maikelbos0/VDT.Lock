using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VDT.Lock.Services;

#if BROWSER
using VDT.Lock.JavascriptInterop;
#endif

namespace VDT.Lock.StorageSites;

public partial class ChromeStorageSite : StorageSiteBase {
    public const int TypeId = 0;

    public static new ChromeStorageSite DeserializeFrom(ReadOnlySpan<byte> plainSpan) {
        var position = 0;

        return new(plainSpan.ReadSpan(ref position));
    }

#if BROWSER
    private const int sectionSize = 1024 * 5;
    private const string headerKey = "Header";
    private const string sectionKey = "Section";
#endif
    public override IEnumerable<int> FieldLengths {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return [0, plainNameBuffer.Value.Length];
        }
    }
    
    public ChromeStorageSite(ReadOnlySpan<byte> plainNameSpan) : base(plainNameSpan) { }

#if BROWSER
    protected override async Task<SecureBuffer?> ExecuteLoad(IStorageSiteServices storageSiteServices) {
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
    protected override Task<SecureBuffer?> ExecuteLoad(IStorageSiteServices storageSiteServices)
        => Task.FromResult<SecureBuffer?>(null);
#endif


#if BROWSER
    protected override async Task<bool> ExecuteSave(SecureBuffer encryptedBuffer, IStorageSiteServices storageSiteServices) {
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
    protected override Task<bool> ExecuteSave(SecureBuffer encryptedBuffer, IStorageSiteServices storageSiteServices)
        => Task.FromResult(false);
#endif

    public override void SerializeTo(SecureByteList plainBytes) {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        plainBytes.WriteInt(this.GetLength());
        plainBytes.WriteInt(TypeId);
        plainBytes.WriteSecureBuffer(plainNameBuffer);
    }
}
