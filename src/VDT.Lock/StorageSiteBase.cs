using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VDT.Lock.StorageSites;

namespace VDT.Lock;

public abstract class StorageSiteBase : IData<StorageSiteBase>, IDisposable {
    public static StorageSiteBase DeserializeFrom(ReadOnlySpan<byte> plainSpan) {
        var position = 0;
        var typeId = plainSpan.ReadInt(ref position);

        return typeId switch {
            0 => ChromeStorageSite.DeserializeFrom(plainSpan[position..]),
            1 => FileSystemStorageSite.DeserializeFrom(plainSpan[position..]),
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

    public abstract void SerializeTo(SecureByteList plainBytes);

    public virtual void Dispose() {
        plainNameBuffer.Dispose();
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }
}
