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
            HistoryItems = DataCollection<DataItem>.DeserializeFrom(plainSpan.ReadSpan(ref position))
        };
    }

    public static DataItem Merge(IEnumerable<DataItem> candidates) {
        var result = DataIdentity.SelectNewest(candidates);

        result.Fields = DataCollection.Merge(candidates.Select(candidate => candidate.Fields));
        result.Labels = DataCollection.Merge(candidates.Select(candidate => candidate.labels));
        result.Locations = DataCollection.Merge(candidates.Select(candidate => candidate.locations));

        foreach (var candidate in candidates) {
            if (candidate != result) {
                if (result.historyItems.Any(historyItem => historyItem.Identity.Version.SequenceEqual(candidate.Identity.Version))) {
                    candidate.Dispose();
                }
                else {
                    foreach (var candidateHistoryItem in candidate.HistoryItems.UnsafeClear()) {
                        if (!result.historyItems.Any(historyItem => historyItem.Identity.Version.SequenceEqual(candidateHistoryItem.Identity.Version))) {
                            result.HistoryItems.Add(candidateHistoryItem);
                        }
                        else {
                            candidateHistoryItem.Dispose();
                        }
                    }
                    result.historyItems.Add(candidate);
                }
            }
        }

        return result;
    }

    private readonly DataIdentity identity;
    private SecureBuffer plainNameBuffer;
    private DataCollection<DataField> fields = [];
    private DataCollection<DataValue> labels = [];
    private DataCollection<DataValue> locations = [];
    private DataCollection<DataItem> historyItems = [];

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

    public DataCollection<DataItem> HistoryItems {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return historyItems;
        }
        set {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            historyItems.Dispose();
            historyItems = value;
        }
    }

    public IEnumerable<int> FieldLengths {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return [identity.Length, plainNameBuffer.Value.Length, fields.Length, labels.Length, locations.Length, historyItems.Length];
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

        plainBytes.WriteInt(this.Length);
        identity.SerializeTo(plainBytes);
        plainBytes.WriteSecureBuffer(plainNameBuffer);
        fields.SerializeTo(plainBytes);
        labels.SerializeTo(plainBytes);
        locations.SerializeTo(plainBytes);
        historyItems.SerializeTo(plainBytes);
    }

    public void Dispose() {
        identity.Dispose();
        plainNameBuffer.Dispose();
        fields.Dispose();
        labels.Dispose();
        locations.Dispose();
        historyItems.Dispose();
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }
}
