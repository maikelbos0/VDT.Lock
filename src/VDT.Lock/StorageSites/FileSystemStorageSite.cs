#if !BROWSER
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

    public override SecureBuffer Load() {
        throw new NotImplementedException();
    }

    public override void Save() {
        throw new NotImplementedException();
    }
}
#endif
