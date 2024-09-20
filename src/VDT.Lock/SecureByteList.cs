using System.Security.Cryptography;

namespace VDT.Lock;

public sealed class SecureByteList : IDisposable {
    public const int DefaultCapacity = 64;

    private readonly object listLock = new();
    private SecureBuffer buffer;
    private int length = 0;

    public static int GetCapacity(int requestedCapacity) {
        var capacity = DefaultCapacity;

        while (capacity < requestedCapacity) {
            capacity *= 2;
        }

        return Math.Min(capacity, Array.MaxLength);
    }

    public SecureByteList() {
        buffer = new(new byte[DefaultCapacity]);
    }

    public SecureByteList(Stream stream) {
        int bytesRead;

        buffer = new(new byte[GetCapacity(stream.CanRead ? (int)stream.Length + DefaultCapacity : DefaultCapacity)]);

        do {
            EnsureCapacity(length + DefaultCapacity);
            bytesRead = stream.Read(buffer.Value, length, DefaultCapacity);
            length += bytesRead;
        } while (bytesRead > 0);
    }

    public SecureByteList(byte[] bytes) {
        buffer = new(bytes);
        length = bytes.Length;
    }

    public void Add(char c) => Add((byte)c);

    public void Add(byte b) {
        lock (listLock) {
            EnsureCapacity(length + 1);
            buffer.Value[length++] = b;
        }
    }

    public void EnsureCapacity(int requestedCapacity) {
        if (buffer.Value.Length < requestedCapacity && buffer.Value.Length < Array.MaxLength) {
            var capacity = GetCapacity(requestedCapacity);
            var newBytes = new byte[capacity];

            Buffer.BlockCopy(buffer.Value, 0, newBytes, 0, buffer.Value.Length);
            buffer.Dispose();
            buffer = new(newBytes);
        }
    }

    public void RemoveLast() {
        lock (listLock) {
            CryptographicOperations.ZeroMemory(new Span<byte>(buffer.Value, --length, 1));
        }
    }

    public void Clear() {
        lock (listLock) {
            CryptographicOperations.ZeroMemory(buffer.Value);
            length = 0;
        }
    }

    public ReadOnlySpan<byte> GetValue() => new(buffer.Value, 0, length);

    public SecureBuffer GetBuffer() {
        var bytes = new byte[length];

        Buffer.BlockCopy(buffer.Value, 0, bytes, 0, length);
        
        return new SecureBuffer(bytes);
    }

    public void Dispose() {
        buffer.Dispose();
        GC.SuppressFinalize(this);
    }
}
