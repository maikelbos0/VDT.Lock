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
        var plainNameSpan = plainSpan.ReadSpan(ref position);
        var storageSettings = StorageSettings.DeserializeFrom(plainSpan.ReadSpan(ref position));

        return typeName switch {
            nameof(FileSystemStorageSite) => new FileSystemStorageSite(plainNameSpan, storageSettings),
            nameof(ChromeStorageSite) => new ChromeStorageSite(plainNameSpan, storageSettings),
            _ => throw new NotImplementedException($"No implementation found for '{typeName}'.")
        };
    }

    protected SecureBuffer plainNameBuffer;
    protected readonly StorageSettings storageSettings;

    public bool IsDisposed { get; private set; }

    public ReadOnlySpan<byte> Name {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return new(plainNameBuffer.Value);
        }
        set {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            plainNameBuffer.Dispose();
            plainNameBuffer = new(value.ToArray());
        }
    }

    public virtual IEnumerable<int> FieldLengths {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return [Encoding.UTF8.GetByteCount(GetType().Name), plainNameBuffer.Value.Length, storageSettings.GetLength()];
        }
    }

    public StorageSiteBase(ReadOnlySpan<byte> plainNameSpan, StorageSettings storageSettings) {
        plainNameBuffer = new(plainNameSpan.ToArray());
        this.storageSettings = storageSettings;
    }

    public Task<SecureBuffer?> Load() {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        return ExecuteLoad();
    }

    protected abstract Task<SecureBuffer?> ExecuteLoad();

    public Task<bool> Save(SecureBuffer encryptedBuffer) {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        return ExecuteSave(encryptedBuffer);
    }

    protected abstract Task<bool> ExecuteSave(SecureBuffer encryptedSpan);

    public void SerializeTo(SecureByteList plainBytes) {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        plainBytes.WriteInt(this.GetLength());
        plainBytes.WriteString(GetType().Name);
        plainBytes.WriteSecureBuffer(plainNameBuffer);
        storageSettings.SerializeTo(plainBytes);
    }

    public void Dispose() {
        plainNameBuffer.Dispose();
        //storageSettings.Dispose();
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }
}
