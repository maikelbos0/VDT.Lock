using System;
using System.Threading.Tasks;

namespace VDT.Lock.StorageSites;

public class FileSystemStorageSite : StorageSiteBase {
    public FileSystemStorageSite(ReadOnlySpan<byte> plainNameSpan, StorageSettings storageSettings) : base(plainNameSpan, storageSettings) { }

    public FileSystemStorageSite(ReadOnlySpan<byte> plainNameSpan, ReadOnlySpan<byte> location) : base(plainNameSpan, new StorageSettings()) {
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
