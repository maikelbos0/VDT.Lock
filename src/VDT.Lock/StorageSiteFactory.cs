using System;
using VDT.Lock.StorageSites;

namespace VDT.Lock;

public class StorageSiteFactory : IStorageSiteFactory {
    public StorageSiteBase Create(string typeName, StorageSettings settings)
        => typeName switch {
#if !BROWSER
            nameof(FileSystemStorageSite) => new FileSystemStorageSite(settings),
#else
            nameof(ChromeStorageSite) => new ChromeStorageSite(settings),
#endif
            _ => throw new NotImplementedException($"No implementation found for '{typeName}'.")
        };
}
