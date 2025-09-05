using System;
using System.IO;
using System.Text;
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
        using var fileStream = File.OpenRead(Encoding.UTF8.GetString(Location));
        using var encryptedBytes = new SecureByteList(fileStream);

        return Task.FromResult(encryptedBytes.ToBuffer());
    }

    protected override Task ExecuteSave(ReadOnlySpan<byte> encryptedSpan) {
        using var fileStream = File.Create(Encoding.UTF8.GetString(Location));

        fileStream.Write(encryptedSpan);

        return Task.CompletedTask;
    }
}
