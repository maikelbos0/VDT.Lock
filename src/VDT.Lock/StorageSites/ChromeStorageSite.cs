#if BROWSER
using System;

namespace VDT.Lock.StorageSites;

public class ChromeStorageSite : StorageSiteBase {
    public ChromeStorageSite(StorageSettings storageSettings) : base(storageSettings) { }

    public override SecureBuffer Load() {
        throw new NotImplementedException();
    }

    public override void Save() {
        throw new NotImplementedException();
    }
}
#endif
