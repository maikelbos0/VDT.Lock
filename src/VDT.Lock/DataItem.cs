using System;

namespace VDT.Lock;

public sealed class DataItem : IDisposable {
    private SecureBuffer plainNameBuffer;
    private readonly DataCollection<DataField> fields = [];

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

    public DataCollection<DataField> Fields {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return fields;
        }
    }

    public DataItem() : this(ReadOnlySpan<byte>.Empty) { }

    public DataItem(ReadOnlySpan<byte> plainNameSpan) {
        plainNameBuffer = new(plainNameSpan.ToArray());
    }

    public void Dispose() {
        plainNameBuffer.Dispose();
        Fields.Dispose();
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }
}
