using System;

namespace VDT.Lock;

public sealed class DataStore : IData, IDisposable {
    private SecureBuffer plainNameBuffer;
    private readonly DataCollection<DataItem> items = [];

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

    public DataCollection<DataItem> Items {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return items;
        }
    }

    public int Length {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return plainNameBuffer.Value.Length
                + items.Length
                + 8;
        }
    }

    public DataStore() : this(ReadOnlySpan<byte>.Empty) { }

    public DataStore(ReadOnlySpan<byte> plainValueSpan) {
        plainNameBuffer = new(plainValueSpan.ToArray());
    }

    public void SerializeTo(SecureByteList plainBytes) {
        throw new NotImplementedException();
    }

    public void Dispose() {
        plainNameBuffer.Dispose();
        items.Dispose();
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }
}
