using System;
using System.Collections.Generic;

namespace VDT.Lock;

public sealed class DataItem : IData<DataItem>, IDisposable {
    public static DataItem DeserializeFrom(ReadOnlySpan<byte> plainSpan) {
        var position = 0;
        var dataItem = new DataItem(plainSpan.ReadSpan(ref position));
        var plainFieldsSpan = plainSpan.ReadSpan(ref position);
        var plainLabelsSpan = plainSpan.ReadSpan(ref position);
        var plainLocationsSpan = plainSpan.ReadSpan(ref position);

        // TODO add collection deserialize
        position = 0;
        while (position < plainFieldsSpan.Length) {
            dataItem.Fields.Add(DataField.DeserializeFrom(plainFieldsSpan.ReadSpan(ref position)));
        }

        position = 0;
        while (position < plainLabelsSpan.Length) {
            dataItem.labels.Add(DataValue.DeserializeFrom(plainLabelsSpan.ReadSpan(ref position)));
        }

        position = 0;
        while (position < plainLocationsSpan.Length) {
            dataItem.Locations.Add(DataValue.DeserializeFrom(plainLocationsSpan.ReadSpan(ref position)));
        }

        return dataItem;
    }

    private SecureBuffer plainNameBuffer;
    private readonly DataCollection<DataField> fields = [];
    private readonly DataCollection<DataValue> labels = [];
    private readonly DataCollection<DataValue> locations = [];

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

    public DataCollection<DataValue> Labels {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return labels;
        }
    }

    public DataCollection<DataValue> Locations {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return locations;
        }
    }

    public IEnumerable<int> FieldLengths {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return [plainNameBuffer.Length, fields.GetLength(), labels.GetLength(), locations.GetLength()];
        }
    }

    public DataItem() : this([]) { }

    public DataItem(ReadOnlySpan<byte> plainNameSpan) {
        plainNameBuffer = new(plainNameSpan.ToArray());
    }

    public void SerializeTo(SecureByteList plainBytes) {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        plainBytes.WriteInt(this.GetLength());
        plainBytes.WriteSecureBuffer(plainNameBuffer);
        fields.SerializeTo(plainBytes);
        labels.SerializeTo(plainBytes);
        locations.SerializeTo(plainBytes);
    }

    public void Dispose() {
        plainNameBuffer.Dispose();
        fields.Dispose();
        labels.Dispose();
        locations.Dispose();
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }
}
