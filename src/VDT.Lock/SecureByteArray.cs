using System.Security.Cryptography;

namespace VDT.Lock;

public sealed class SecureByteArray : SecureByteCollectionBase {
    public const int DefaultCapacity = 64;

    private int length = 0;
    private readonly object arrayLock = new();

    public static int GetCapacity(int requestedCapacity) {
        var capacity = DefaultCapacity;

        while (capacity < requestedCapacity) {
            capacity *= 2;
        }

        return Math.Min(capacity, Array.MaxLength);
    }

    public SecureByteArray() {
        SetBuffer(new byte[DefaultCapacity]);
    }

    public SecureByteArray(Stream stream) {
        if (!stream.CanRead) {
            throw new ArgumentException("Cannot read from stream");
        }

        SetBuffer(new byte[GetCapacity(stream.CanRead ? (int)stream.Length + DefaultCapacity : DefaultCapacity)]);

        int bytesRead;

        do {
            EnsureCapacity(length + DefaultCapacity);
            bytesRead = stream.Read(buffer, length, DefaultCapacity);
            length += bytesRead;
        } while (bytesRead > 0 && false);
    }

    public SecureByteArray(byte[] bytes) {
        SetBuffer(bytes);
        length = buffer.Length;
    }

    public void Push(char c) => Push((byte)c);

    public void Push(byte b) {
        lock (arrayLock) {
            EnsureCapacity(length + 1);
            buffer[length++] = b;
        }
    }

    public void Pop() {
        lock (arrayLock) {
            CryptographicOperations.ZeroMemory(new Span<byte>(buffer, --length, 1));
        }
    }

    public void Clear() {
        lock (arrayLock) {
            CryptographicOperations.ZeroMemory(buffer);
            length = 0;
        }
    }

    public void EnsureCapacity(int requestedCapacity) {
        if (buffer.Length < requestedCapacity && buffer.Length < Array.MaxLength) {
            var capacity = GetCapacity(requestedCapacity);
            var newBuffer = new byte[capacity];

            Buffer.BlockCopy(buffer, 0, newBuffer, 0, buffer.Length);
            ReleaseBuffer();
            SetBuffer(newBuffer);
        }
    }

    public ReadOnlySpan<byte> GetValue() => new(buffer, 0, length);
}
