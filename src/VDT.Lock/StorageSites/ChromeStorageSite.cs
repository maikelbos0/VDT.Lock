#if BROWSER
using System;
using System.Threading.Tasks;

namespace VDT.Lock.StorageSites;

public class ChromeStorageSite : StorageSiteBase {
    public ChromeStorageSite(StorageSettings storageSettings) : base(storageSettings) { }

    protected override Task<SecureBuffer> ExecuteLoad() {
        throw new NotImplementedException();
    }

    protected override Task ExecuteSave(ReadOnlySpan<byte> encryptedData) {
        throw new NotImplementedException();
    }
}
#endif
