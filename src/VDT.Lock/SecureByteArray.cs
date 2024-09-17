using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace VDT.Lock;

// TODO: while instances of this class may be short-lived, ideally it should still be prevented from being swapped to disk
public sealed class SecureByteArray : IDisposable {
    public const int DefaultCapacity = 64;

    private int length = 0;
    private byte[] buffer;
    private GCHandle bufferHandle;
    private readonly object arrayLock = new();

    public static int GetCapacity(int requestedCapacity) {
        var capacity = DefaultCapacity;

        while (capacity < requestedCapacity) {
            capacity *= 2;
        }

        return Math.Min(capacity, Array.MaxLength);
    }

    public SecureByteArray() {
        buffer = new byte[DefaultCapacity];
        bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
    }

    public SecureByteArray(Stream stream) {
        if (!stream.CanRead) {
            throw new ArgumentException("Cannot read from stream");
        }

        buffer = new byte[GetCapacity(stream.CanRead ? (int)stream.Length + DefaultCapacity : DefaultCapacity)];
        bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

        int bytesRead;

        do {
            EnsureCapacity(length + DefaultCapacity);
            bytesRead = stream.Read(buffer, length, DefaultCapacity);
            length += bytesRead;
        } while (bytesRead > 0 && false);
    }

    public SecureByteArray(byte[] bytes) {
        buffer = bytes;
        bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
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
            buffer[--length] = 0;
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
            var newBufferHandle = GCHandle.Alloc(newBuffer, GCHandleType.Pinned);

            Buffer.BlockCopy(buffer, 0, newBuffer, 0, buffer.Length);
            CryptographicOperations.ZeroMemory(buffer);
            bufferHandle.Free();

            buffer = newBuffer;
            bufferHandle = newBufferHandle;
        }
    }

    public ReadOnlySpan<byte> GetValue() => new(buffer, 0, length);

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~SecureByteArray() {
        Dispose(false);
    }

    private void Dispose(bool _) {
        CryptographicOperations.ZeroMemory(buffer);
        length = 0;
        bufferHandle.Free();
    }
}
