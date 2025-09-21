using System;
using System.Collections.Generic;

namespace VDT.Lock;

public sealed class DataIdentity : IData<DataIdentity>, IDisposable {
    public static DataIdentity DeserializeFrom(ReadOnlySpan<byte> plainSpan) {
        var position = 0;
        
        return new(plainSpan.ReadSpan(ref position), plainSpan.ReadSpan(ref position));
    }
    
    private SecureBuffer plainKeyBuffer;
    private SecureBuffer plainVersionBuffer;

    public bool IsDisposed { get; private set; }

    public ReadOnlySpan<byte> Key {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return new(plainKeyBuffer.Value);
        }
    }

    public ReadOnlySpan<byte> Version {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return new(plainVersionBuffer.Value);
        }
    }

    public IEnumerable<int> FieldLengths {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return [plainKeyBuffer.Value.Length, plainVersionBuffer.Value.Length];
        }
    }

    public DataIdentity() {
        plainKeyBuffer = new(Guid.NewGuid().ToByteArray());
        plainVersionBuffer = new(DateTimeOffset.UtcNow.ToVersion());
    }

    private DataIdentity(ReadOnlySpan<byte> plainKeySpan, ReadOnlySpan<byte> plainVersionSpan) {
        plainKeyBuffer = new(plainKeySpan.ToArray());
        plainVersionBuffer = new(plainVersionSpan.ToArray());
    }

    public void SerializeTo(SecureByteList plainBytes) {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        plainBytes.WriteInt(this.GetLength());
        plainBytes.WriteSecureBuffer(plainKeyBuffer);
        plainBytes.WriteSecureBuffer(plainVersionBuffer);
    }

    public void Dispose() {
        plainKeyBuffer.Dispose();
        plainVersionBuffer.Dispose();
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }
}
