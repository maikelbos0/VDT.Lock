using System;
using System.Collections.Generic;

namespace VDT.Lock;

public sealed class DataStore : IData, IDisposable {
    public static DataStore DeserializeFrom(ReadOnlySpan<byte> plainSpan) {
        var position = 0;
        var dataStore = new DataStore(plainSpan.ReadSpan(ref position));
        var plainItemsSpan = plainSpan.ReadSpan(ref position);

        position = 0;
        while (position < plainItemsSpan.Length) {
            dataStore.Items.Add(DataItem.DeserializeFrom(plainItemsSpan.ReadSpan(ref position)));
        }

        return dataStore;
    }

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

    public IEnumerable<int> FieldLengths {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return [plainNameBuffer.Length, items.GetLength()];
        }
    }

    public DataStore() : this([]) { }

    public DataStore(ReadOnlySpan<byte> plainValueSpan) {
        plainNameBuffer = new(plainValueSpan.ToArray());
    }

    public void SerializeTo(SecureByteList plainBytes) {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        plainBytes.WriteInt(this.GetLength());
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
