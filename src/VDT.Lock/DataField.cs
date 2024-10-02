namespace VDT.Lock;

public sealed class DataField : IDisposable {
    public static DataField DeserializeFrom(ReadOnlySpan<byte> plainSpan) {
        var position = 0;

        return new(plainSpan.ReadSpan(ref position), plainSpan.ReadSpan(ref position));
    }

    private SecureBuffer plainNameBuffer;
    private readonly object plainNameBufferLock = new();
    private SecureBuffer plainDataBuffer;
    private readonly object plainDataBufferLock = new();

    public ReadOnlySpan<byte> Name {
        get => new(plainNameBuffer.Value);
        set {
            lock (plainNameBufferLock) {
                plainNameBuffer.Dispose();
                plainNameBuffer = new(value.ToArray());
            }
        }
    }
    public ReadOnlySpan<byte> Data {
        get => new(plainDataBuffer.Value);
        set {
            lock (plainDataBufferLock) {
                plainDataBuffer.Dispose();
                plainDataBuffer = new(value.ToArray());
            }
        }
    }

    public int Length => plainNameBuffer.Value.Length + plainDataBuffer.Value.Length + 8;

    public DataField(ReadOnlySpan<byte> plainNameSpan, ReadOnlySpan<byte> plainDataSpan) {
        this.plainNameBuffer = new(plainNameSpan.ToArray());
        this.plainDataBuffer = new(plainDataSpan.ToArray());
    }

    public void SerializeTo(SecureByteList plainBytes) {
        plainBytes.WriteInt(Length);
        plainBytes.WriteSecureBuffer(plainNameBuffer);
        plainBytes.WriteSecureBuffer(plainDataBuffer);
    }

    public void Dispose() {
        plainNameBuffer.Dispose();
        plainDataBuffer.Dispose();
        GC.SuppressFinalize(this);
    }
}
