using System;

namespace VDT.Lock;

public sealed class DataValue : IDisposable {
    private SecureBuffer plainValueBuffer;

    public bool IsDisposed { get; private set; }

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

            return plainValueBuffer.Value.Length + 4;
        }
    }

    public DataValue() : this(ReadOnlySpan<byte>.Empty) { }

    public DataValue(ReadOnlySpan<byte> plainValueSpan) {
        plainValueBuffer = new(plainValueSpan.ToArray());
    }

    public void Dispose() {
        plainValueBuffer.Dispose();
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }
}
