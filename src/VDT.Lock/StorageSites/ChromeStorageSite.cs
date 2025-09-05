using System;
using System.Threading.Tasks;

namespace VDT.Lock.StorageSites;

public class ChromeStorageSite : StorageSiteBase {
    public ChromeStorageSite(ReadOnlySpan<byte> plainNameSpan, StorageSettings storageSettings) : base(plainNameSpan, storageSettings) { }

    protected override Task<SecureBuffer> ExecuteLoad() {
        throw new NotImplementedException();
    }

    protected override Task ExecuteSave(ReadOnlySpan<byte> encryptedSpan) {
        throw new NotImplementedException();
    }
}
