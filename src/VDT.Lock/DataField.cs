using System;
using System.Collections.Generic;

namespace VDT.Lock;

public sealed class DataField : IData<DataField>, IDisposable {
    public static DataField DeserializeFrom(ReadOnlySpan<byte> plainSpan) {
        var position = 0;

        return new(plainSpan.ReadSpan(ref position), plainSpan.ReadSpan(ref position));
    }

    private SecureBuffer plainNameBuffer;
    private SecureBuffer plainValueBuffer;
    private DataCollection<DataValue> selectors = [];

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

            return [plainNameBuffer.Value.Length, plainValueBuffer.Value.Length];
        }
    }

    public DataField() : this([], []) { }

    public DataField(ReadOnlySpan<byte> plainNameSpan, ReadOnlySpan<byte> plainDataSpan) {
        plainNameBuffer = new(plainNameSpan.ToArray());
        plainValueBuffer = new(plainDataSpan.ToArray());
    }

    public void SerializeTo(SecureByteList plainBytes) {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        plainBytes.WriteInt(this.GetLength());
        plainBytes.WriteSecureBuffer(plainNameBuffer);
        plainBytes.WriteSecureBuffer(plainValueBuffer);
    }

    public void Dispose() {
        plainNameBuffer.Dispose();
        plainValueBuffer.Dispose();
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }
}
