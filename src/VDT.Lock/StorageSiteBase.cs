using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VDT.Lock.StorageSites;

namespace VDT.Lock;

public abstract class StorageSiteBase : IData<StorageSiteBase>, IDisposable {
    public static StorageSiteBase DeserializeFrom(ReadOnlySpan<byte> plainSpan) {
        var position = 0;
        var typeName = plainSpan.ReadString(ref position);
        var storageSettings = StorageSettings.DeserializeFrom(plainSpan.ReadSpan(ref position));

        return typeName switch {
            nameof(FileSystemStorageSite) => new FileSystemStorageSite(storageSettings),
            nameof(ChromeStorageSite) => new ChromeStorageSite(storageSettings),
            _ => throw new NotImplementedException($"No implementation found for '{typeName}'.")
        };
    }

    protected readonly StorageSettings storageSettings;

    public bool IsDisposed { get; private set; }

    public IEnumerable<int> FieldLengths {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return [Encoding.UTF8.GetByteCount(GetType().Name), storageSettings.GetLength()];
        }
    }

    public StorageSiteBase(StorageSettings storageSettings) {
        this.storageSettings = storageSettings;
    }

    public Task<SecureBuffer> Load() {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        return ExecuteLoad();
    }

    protected abstract Task<SecureBuffer> ExecuteLoad();

    public Task Save(ReadOnlySpan<byte> encryptedData) {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        return ExecuteSave(encryptedData);
    }

    protected abstract Task ExecuteSave(ReadOnlySpan<byte> encryptedData);

    public void SerializeTo(SecureByteList plainBytes) {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        plainBytes.WriteInt(this.GetLength());
        plainBytes.WriteSpan(Encoding.UTF8.GetBytes(GetType().Name));
        storageSettings.SerializeTo(plainBytes);
    }

    public void Dispose() {
        storageSettings.Dispose();
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }
}
