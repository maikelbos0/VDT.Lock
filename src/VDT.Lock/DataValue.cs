using System;

namespace VDT.Lock;

public sealed class DataValue : IDisposable {
    public static DataValue DeserializeFrom(ReadOnlySpan<byte> plainSpan) {
        var position = 0;

        return new(plainSpan.ReadSpan(ref position));
    }

    private SecureBuffer plainValueBuffer;

    public ReadOnlySpan<byte> Value {
        get => new(plainValueBuffer.Value);
        set {
            plainValueBuffer.Dispose();
            plainValueBuffer = new(value.ToArray());
        }
    }

    public int Length => plainValueBuffer.Value.Length + 4;

    public DataValue() : this(ReadOnlySpan<byte>.Empty) { }

    public DataValue(ReadOnlySpan<byte> plainValueSpan) {
        plainValueBuffer = new(plainValueSpan.ToArray());
    }

    public void SerializeTo(SecureByteList plainBytes) {
        plainBytes.WriteInt(Length);
        plainBytes.WriteSecureBuffer(plainValueBuffer);
    }

    public void Dispose() {
        plainValueBuffer.Dispose();
        GC.SuppressFinalize(this);
    }
}
