using System;
using System.Collections.Generic;
using System.Linq;

namespace VDT.Lock;

public sealed class DataStore : IData<DataStore>, IIdentifiableData<DataStore>, IDisposable {
    public static DataStore DeserializeFrom(ReadOnlySpan<byte> plainSpan) {
        var position = 0;

        return new DataStore(DataIdentity.DeserializeFrom(plainSpan.ReadSpan(ref position)), plainSpan.ReadSpan(ref position)) {
            Items = DataCollection<DataItem>.DeserializeFrom(plainSpan.ReadSpan(ref position))
        };
    }

    public static DataStore Merge(IEnumerable<DataStore> candidates) {
        var result = DataIdentity.SelectNewest(candidates);

        result.items = DataCollection.Merge(candidates.Select(candidate => candidate.items));

        foreach (var candidate in candidates) {
            if (candidate != result) {
                candidate.Dispose();
            }
        }

        return result;
    }

    private readonly DataIdentity identity;
    private SecureBuffer plainNameBuffer;
    private DataCollection<DataItem> items = [];

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

    public DataCollection<DataItem> Items {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return items;
        }
        set {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            items.Dispose();
            items = value;
            identity.Update();
        }
    }

    public IEnumerable<int> FieldLengths {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return [identity.Length, plainNameBuffer.Value.Length, items.Length];
        }
    }

    public DataStore() : this([]) { }

    public DataStore(ReadOnlySpan<byte> plainValueSpan) : this(new(), plainValueSpan) { }

    public DataStore(DataIdentity identity, ReadOnlySpan<byte> plainValueSpan) {
        this.identity = identity;
        plainNameBuffer = new(plainValueSpan.ToArray());
    }

    public void SerializeTo(SecureByteList plainBytes) {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        // We don't include the length since we're always going to deserialize the entire buffer
        identity.SerializeTo(plainBytes);
        plainBytes.WriteSecureBuffer(plainNameBuffer);
        items.SerializeTo(plainBytes);
    }

    public void Dispose() {
        identity.Dispose();
        plainNameBuffer.Dispose();
        items.Dispose();
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }
}
