using System;
using System.Collections.Generic;

namespace VDT.Lock;

public sealed class DataStore : IData<DataStore>, IDisposable {
    public static DataStore DeserializeFrom(ReadOnlySpan<byte> plainSpan) {
        var position = 0;
        
        return new DataStore(plainSpan.ReadSpan(ref position)) {
            Items = DataCollection<DataItem>.DeserializeFrom(plainSpan.ReadSpan(ref position))
        };
    }

    private SecureBuffer plainNameBuffer;
    private DataCollection<DataItem> items = [];

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
        set {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            items.Dispose();
            items = value;
        }
    }

    public IEnumerable<int> FieldLengths {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return [plainNameBuffer.Value.Length, items.GetLength()];
        }
    }

    public DataStore() : this([]) { }

    public DataStore(ReadOnlySpan<byte> plainValueSpan) {
        plainNameBuffer = new(plainValueSpan.ToArray());
    }

    public void SerializeTo(SecureByteList plainBytes) {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        // We don't include the length since we're always going to deserialize the entire buffer
        plainBytes.WriteSecureBuffer(plainNameBuffer);
        items.SerializeTo(plainBytes);
    }

    public void Dispose() {
        plainNameBuffer.Dispose();
        items.Dispose();
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }
}
