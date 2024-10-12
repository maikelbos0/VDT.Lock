using System;

namespace VDT.Lock;

public sealed class DataItem : IData, IDisposable {
    public static DataItem DeserializeFrom(ReadOnlySpan<byte> plainSpan) {
        var position = 0;
        var dataItem = new DataItem(plainSpan.ReadSpan(ref position));
        var plainFieldsSpan = plainSpan.ReadSpan(ref position);
        var plainLabelsSpan = plainSpan.ReadSpan(ref position);
        var plainLocationsSpan = plainSpan.ReadSpan(ref position);

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

    public int Length {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return plainNameBuffer.Value.Length
                + fields.Length
                + labels.Length
                + locations.Length
                + 16;
        }
    }

    public DataItem() : this(ReadOnlySpan<byte>.Empty) { }

    public DataItem(ReadOnlySpan<byte> plainNameSpan) {
        plainNameBuffer = new(plainNameSpan.ToArray());
    }

    public void SerializeTo(SecureByteList plainBytes) {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        plainBytes.WriteInt(Length);
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
