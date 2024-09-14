using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace VDT.Lock;

// TODO: while instances of this class may be short-lived, ideally it should still be prevented from being swapped to disk
public sealed class SecureByteArray : IDisposable {
    private readonly GCHandle bufferHandle;

    public byte[] Buffer { get; }

    public SecureByteArray(int length) {
        Buffer = new byte[length];
        bufferHandle = GCHandle.Alloc(Buffer, GCHandleType.Pinned);
    }

    [MethodImpl(MethodImplOptions.NoOptimization)]
    public void Clear() {
        for (int i = 0; i < Buffer.Length; i++) {
            Buffer[i] = 0;
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
        Clear();
        bufferHandle.Free();
    }
}
