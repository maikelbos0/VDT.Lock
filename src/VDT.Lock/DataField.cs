using System;

namespace VDT.Lock;

public sealed class DataField : IData, IDisposable {
    public static DataField DeserializeFrom(ReadOnlySpan<byte> plainSpan) {
        var position = 0;

        return new(plainSpan.ReadSpan(ref position), plainSpan.ReadSpan(ref position));
    }

    private SecureBuffer plainNameBuffer;
    private SecureBuffer plainValueBuffer;

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

    public int Length {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return plainNameBuffer.Value.Length + plainValueBuffer.Value.Length + 8;
        }
    }

    public DataField() : this(ReadOnlySpan<byte>.Empty, ReadOnlySpan<byte>.Empty) { }

    public DataField(ReadOnlySpan<byte> plainNameSpan, ReadOnlySpan<byte> plainDataSpan) {
        plainNameBuffer = new(plainNameSpan.ToArray());
        plainValueBuffer = new(plainDataSpan.ToArray());
    }

    public void SerializeTo(SecureByteList plainBytes) {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        plainBytes.WriteInt(Length);
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
