using System;
using System.Threading.Tasks;

namespace VDT.Lock.StorageSites;

public class FileSystemStorageSite : StorageSiteBase {
    public FileSystemStorageSite(StorageSettings storageSettings) : base(storageSettings) { }

    public FileSystemStorageSite(ReadOnlySpan<byte> location) : base(new StorageSettings()) {
        Location = location;
    }

    public ReadOnlySpan<byte> Location {
        get => storageSettings.Get(nameof(Location));
        set => storageSettings.Set(nameof(Location), value);
    }

    protected override Task<SecureBuffer> ExecuteLoad() {
        throw new NotImplementedException();
    }

    protected override Task ExecuteSave(ReadOnlySpan<byte> encryptedData) {
        throw new NotImplementedException();
    }
}
