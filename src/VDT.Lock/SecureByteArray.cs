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
        buffer = new byte[capacity];
        bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
    }

    public void Push(char c) => Push((byte)c);

    public void Push(byte b) {
        lock (arrayLock) {
            buffer[length++] = b;
        }
    }

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

    public ReadOnlySpan<byte> GetValue() => new(buffer, 0, length);

    private void ClearBuffer() {
        for (int i = 0; i < buffer.Length; i++) {
            buffer[i] = 0;
        }
        length = 0;
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
        ClearBuffer();
        length = 0;
        bufferHandle.Free();
    }
}
