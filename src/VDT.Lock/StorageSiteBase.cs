using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VDT.Lock.Services;
using VDT.Lock.StorageSites;

namespace VDT.Lock;

public abstract class StorageSiteBase : IData<StorageSiteBase>, IDisposable {
    public static StorageSiteBase DeserializeFrom(ReadOnlySpan<byte> plainSpan) {
        var position = 0;
        var typeId = plainSpan.ReadInt(ref position);

        return typeId switch {
            ChromeStorageSite.TypeId => ChromeStorageSite.DeserializeFrom(plainSpan[position..]),
            FileSystemStorageSite.TypeId => FileSystemStorageSite.DeserializeFrom(plainSpan[position..]),
            ApiStorageSite.TypeId => ApiStorageSite.DeserializeFrom(plainSpan[position..]),
            _ => throw new NotImplementedException($"No implementation found for type '{typeId}'.")
        };
    }

    protected SecureBuffer plainNameBuffer;

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

    public abstract IEnumerable<int> FieldLengths { get; }

    public StorageSiteBase(ReadOnlySpan<byte> plainNameSpan) {
        plainNameBuffer = new(plainNameSpan.ToArray());
    }

    public Task<SecureBuffer?> Load(IStorageSiteServices storageSiteServices) {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        return ExecuteLoad(storageSiteServices);
    }

    protected abstract Task<SecureBuffer?> ExecuteLoad(IStorageSiteServices storageSiteServices);

    public Task<bool> Save(SecureBuffer encryptedBuffer, IStorageSiteServices storageSiteServices) {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        return ExecuteSave(encryptedBuffer, storageSiteServices);
    }

    protected abstract Task<bool> ExecuteSave(SecureBuffer encryptedSpan, IStorageSiteServices storageSiteServices);

    public abstract void SerializeTo(SecureByteList plainBytes);

    public virtual void Dispose() {
        plainNameBuffer.Dispose();
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }
}
