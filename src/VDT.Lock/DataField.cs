namespace VDT.Lock;

public sealed class DataField : IDisposable {
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

    public DataField(SecureBuffer plainNameBuffer, SecureBuffer plainDataBuffer) {
        this.plainNameBuffer = plainNameBuffer;
        this.plainDataBuffer = plainDataBuffer;
    }

    public DataField(ReadOnlySpan<byte> plainSpan) {
        var position = 0;

        plainNameBuffer = plainSpan.ReadSecureBuffer(ref position);
        plainDataBuffer = plainSpan.ReadSecureBuffer(ref position);
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
