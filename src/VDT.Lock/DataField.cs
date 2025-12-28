using System;
using System.Collections.Generic;
using System.Linq;

namespace VDT.Lock;

public sealed class DataField : IData<DataField>, IIdentifiableData<DataField>, IDisposable {
    public static DataField DeserializeFrom(ReadOnlySpan<byte> plainSpan) {
        var position = 0;

        return new(DataIdentity.DeserializeFrom(plainSpan.ReadSpan(ref position)), plainSpan.ReadSpan(ref position), plainSpan.ReadSpan(ref position)) {
            Selectors = DataCollection<DataValue>.DeserializeFrom(plainSpan.ReadSpan(ref position))
        };
    }

    public static DataField Merge(IEnumerable<DataField> candidates) {
        var result = DataIdentity.SelectNewest(candidates);

        result.Selectors = DataCollection.Merge(candidates.Select(candidate => candidate.Selectors));

        foreach (var candidate in candidates) {
            if (candidate != result) {
                candidate.Dispose();
            }
        }

        return result;
    }

    private readonly DataIdentity identity;
    private SecureBuffer plainNameBuffer;
    private SecureBuffer plainValueBuffer;
    private DataCollection<DataValue> selectors = [];

    public bool IsDisposed { get; private set; }

    public DataIdentity Identity {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return identity;
        }
    }

    public ReadOnlySpan<byte> Name {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return new(plainNameBuffer.Value);
        }
        set {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            plainNameBuffer.Dispose();
            plainNameBuffer = new(value.ToArray());
            identity.Update();
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
            identity.Update();
        }
    }

    public DataCollection<DataValue> Selectors {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return selectors;
        }
        set {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            selectors.Dispose();
            selectors = value;
        }
    }

    public IEnumerable<int> FieldLengths {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return [identity.GetLength(), plainNameBuffer.Value.Length, plainValueBuffer.Value.Length, selectors.GetLength()];
        }
    }

    public DataField() : this([], []) { }

    public DataField(ReadOnlySpan<byte> plainNameSpan, ReadOnlySpan<byte> plainDataSpan) : this(new(), plainNameSpan, plainDataSpan) { }

    public DataField(DataIdentity identity, ReadOnlySpan<byte> plainNameSpan, ReadOnlySpan<byte> plainDataSpan) {
        this.identity = identity;
        plainNameBuffer = new(plainNameSpan.ToArray());
        plainValueBuffer = new(plainDataSpan.ToArray());
    }

    public void SerializeTo(SecureByteList plainBytes) {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        plainBytes.WriteInt(this.GetLength());
        identity.SerializeTo(plainBytes);
        plainBytes.WriteSecureBuffer(plainNameBuffer);
        plainBytes.WriteSecureBuffer(plainValueBuffer);
        selectors.SerializeTo(plainBytes);
    }

    public void Dispose() {
        identity.Dispose();
        plainNameBuffer.Dispose();
        plainValueBuffer.Dispose();
        selectors.Dispose();
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }
}
