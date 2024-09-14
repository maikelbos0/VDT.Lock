using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace VDT.Lock;

// TODO: while instances of this class may be short-lived, ideally it should still be prevented from being swapped to disk
public sealed class SecureByteArray : IDisposable {
    public const int DefaultCapacity = 4;

    private int length = 0;
    private byte[] buffer;
    private GCHandle bufferHandle;
    private readonly object arrayLock = new();

    public SecureByteArray(int capacity = DefaultCapacity) {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);

        buffer = new byte[capacity];
        bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
    }

    public void Push(char c) => Push((byte)c);

    public void Push(byte b) {
        lock (arrayLock) {
            EnsureCapacity(length + 1);
            buffer[length++] = b;
        }
    }

    //public void CopyFrom(Stream stream) {

    //}

    public void Pop() {
        lock (arrayLock) {
            buffer[--length] = 0;
        }
    }

    [MethodImpl(MethodImplOptions.NoOptimization)]
    public void Clear() {
        lock (arrayLock) {
            ClearBuffer();
            length = 0;
        }
    }

    public void EnsureCapacity(int requestedCapacity) {
        if (buffer.Length < requestedCapacity && buffer.Length < Array.MaxLength) {
            var capacity = Math.Max(Math.Min(2 * buffer.Length, Array.MaxLength), DefaultCapacity);
            var newBuffer = new byte[capacity];
            var newBufferHandle = GCHandle.Alloc(newBuffer, GCHandleType.Pinned);

            Buffer.BlockCopy(buffer, 0, newBuffer, 0, buffer.Length);
            ClearBuffer();
            bufferHandle.Free();

            buffer = newBuffer;
            bufferHandle = newBufferHandle;
        }
    }

    public ReadOnlySpan<byte> GetValue() => new(buffer, 0, length);

    private void ClearBuffer() {
        for (int i = 0; i < buffer.Length; i++) {
            buffer[i] = 0;
        }
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~SecureByteArray() {
        Dispose(false);
    }

    private void Dispose(bool _) {
        ClearBuffer();
        length = 0;
        bufferHandle.Free();
    }
}
