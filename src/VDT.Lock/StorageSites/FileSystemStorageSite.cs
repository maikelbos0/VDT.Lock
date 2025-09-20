using System;
using System.Threading.Tasks;

#if !BROWSER
using System.IO;
using System.Text;
#endif

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

#if BROWSER
    protected override Task<SecureBuffer?> ExecuteLoad()
        => Task.FromResult<SecureBuffer?>(null);
#else
    protected override Task<SecureBuffer?> ExecuteLoad() {
        using var fileStream = File.OpenRead(Encoding.UTF8.GetString(Location));
        using var encryptedBytes = new SecureByteList(fileStream);

        return Task.FromResult<SecureBuffer?>(encryptedBytes.ToBuffer());
    }
#endif

#if BROWSER
    protected override Task<bool> ExecuteSave(SecureBuffer encryptedBuffer)
        => Task.FromResult(false);
#else
    protected override Task<bool> ExecuteSave(SecureBuffer encryptedBuffer) {
        using var fileStream = File.Create(Encoding.UTF8.GetString(Location));

        fileStream.Write(encryptedBuffer.Value);

        return Task.FromResult(true);
    }
#endif
}
