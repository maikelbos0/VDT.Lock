using System;
using System.Threading.Tasks;

namespace VDT.Lock.StorageSites;

public class ChromeStorageSite : StorageSiteBase {
    public ChromeStorageSite(ReadOnlySpan<byte> plainNameSpan, StorageSettings storageSettings) : base(plainNameSpan, storageSettings) { }

    protected override Task<SecureBuffer?> ExecuteLoad() {
        return Task.FromResult<SecureBuffer?>(null);
    }

    protected override Task<bool> ExecuteSave(ReadOnlySpan<byte> encryptedSpan) {
        return Task.FromResult(false);
    }
}
