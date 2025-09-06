using System;
using System.Text;
using VDT.Lock.StorageSites;

namespace VDT.Lock;

public class StorageSiteFactory : IStorageSiteFactory {
    public StorageSiteBase DeserializeFrom(ReadOnlySpan<byte> plainSpan) {
        var position = 0;
        var typeName = Encoding.UTF8.GetString(plainSpan.ReadSpan(ref position));
        var storageSettings = StorageSettings.DeserializeFrom(plainSpan.ReadSpan(ref position));

        // TODO if we can create a static interface method, perhaps we don't need the switch
        return typeName switch {
#if !BROWSER
            nameof(FileSystemStorageSite) => new FileSystemStorageSite(storageSettings),
#else
            nameof(ChromeStorageSite) => new ChromeStorageSite(storageSettings),
#endif
            _ => throw new NotImplementedException($"No implementation found for '{typeName}'.")
        };
    }
}
