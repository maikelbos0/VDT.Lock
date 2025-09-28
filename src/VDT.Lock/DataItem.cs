using System;
using System.Collections.Generic;
using System.Linq;

namespace VDT.Lock;

public sealed class DataItem : IData<DataItem>, IIdentifiableData<DataItem>, IDisposable {
    public static DataItem DeserializeFrom(ReadOnlySpan<byte> plainSpan) {
        var position = 0;

        return new DataItem(DataIdentity.DeserializeFrom(plainSpan.ReadSpan(ref position)), plainSpan.ReadSpan(ref position)) {
            Fields = DataCollection<DataField>.DeserializeFrom(plainSpan.ReadSpan(ref position)),
            Labels = DataCollection<DataValue>.DeserializeFrom(plainSpan.ReadSpan(ref position)),
            Locations = DataCollection<DataValue>.DeserializeFrom(plainSpan.ReadSpan(ref position)),
        };
    }

    public static DataItem Merge(IEnumerable<DataItem> candidates) {
        var result = DataIdentity.SelectNewest(candidates);

        result.Fields = DataCollection.Merge(candidates.Select(candidate => candidate.Fields));
        result.Labels = DataCollection.Merge(candidates.Select(candidate => candidate.labels));
        result.Locations = DataCollection.Merge(candidates.Select(candidate => candidate.locations));

        foreach (var candidate in candidates) {
            if (candidate != result) {
                candidate.Dispose();
            }
        }

        return result;
    }

    private readonly DataIdentity identity;
    private SecureBuffer plainNameBuffer;
    private DataCollection<DataField> fields = [];
    private DataCollection<DataValue> labels = [];
    private DataCollection<DataValue> locations = [];

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
        }
    }

    public DataCollection<DataField> Fields {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return fields;
        }
        set {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            fields.Dispose();
            fields = value;
        }
    }

    public DataCollection<DataValue> Labels {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return labels;
        }
        set {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            labels.Dispose();
            labels = value;
        }
    }

    public DataCollection<DataValue> Locations {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return locations;
        }
        set {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            locations.Dispose();
            locations = value;
        }
    }

    public IEnumerable<int> FieldLengths {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return [identity.GetLength(), plainNameBuffer.Value.Length, fields.GetLength(), labels.GetLength(), locations.GetLength()];
        }
    }

    public DataItem() : this([]) { }

    public DataItem(ReadOnlySpan<byte> plainNameSpan) : this(new(), plainNameSpan) { }

    public DataItem(DataIdentity identity, ReadOnlySpan<byte> plainNameSpan) {
        this.identity = identity;
        plainNameBuffer = new(plainNameSpan.ToArray());
    }

    public void SerializeTo(SecureByteList plainBytes) {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        plainBytes.WriteInt(this.GetLength());
        identity.SerializeTo(plainBytes);
        plainBytes.WriteSecureBuffer(plainNameBuffer);
        fields.SerializeTo(plainBytes);
        labels.SerializeTo(plainBytes);
        locations.SerializeTo(plainBytes);
    }

    public void Dispose() {
        identity.Dispose();
        plainNameBuffer.Dispose();
        fields.Dispose();
        labels.Dispose();
        locations.Dispose();
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }
}
