using System;
using System.Collections.Generic;

namespace VDT.Lock;

public sealed class DataValue : IData<DataValue>, IIdentifiableData, IDisposable {
    public static DataValue DeserializeFrom(ReadOnlySpan<byte> plainSpan) {
        var position = 0;

        return new(DataIdentity.DeserializeFrom(plainSpan.ReadSpan(ref position)), plainSpan.ReadSpan(ref position));
    }

    public static DataValue Merge(IEnumerable<DataValue> candidates) {
        var result = DataIdentity.SelectNewest(candidates);

        foreach (var candidate in candidates) {
            if (candidate != result) {
                candidate.Dispose();
            }
        }

        return result;
    }

    private readonly DataIdentity identity;
    private SecureBuffer plainValueBuffer;

    public bool IsDisposed { get; private set; }

    public DataIdentity Identity {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return identity;
        }
    }

    public ReadOnlySpan<byte> Value {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return new(plainValueBuffer.Value);
        }
        set {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            plainValueBuffer.Dispose();
            plainValueBuffer = new(value.ToArray());
        }
    }

    public IEnumerable<int> FieldLengths {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return [Identity.GetLength(), plainValueBuffer.Value.Length];
        }
    }

    public DataValue() : this([]) { }

    public DataValue(ReadOnlySpan<byte> plainValueSpan) : this(new(), plainValueSpan) { }

    public DataValue(DataIdentity identity, ReadOnlySpan<byte> plainValueSpan) {
        this.identity = identity;
        plainValueBuffer = new(plainValueSpan.ToArray());
    }

    public void SerializeTo(SecureByteList plainBytes) {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        plainBytes.WriteInt(this.GetLength());
        Identity.SerializeTo(plainBytes);
        plainBytes.WriteSecureBuffer(plainValueBuffer);
    }

    public void Dispose() {
        identity.Dispose();
        plainValueBuffer.Dispose();
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }
}
