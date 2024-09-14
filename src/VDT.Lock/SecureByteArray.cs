using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace VDT.Lock;

public sealed class SecureByteArray : IDisposable {
    private readonly GCHandle bufferHandle;

    public byte[] Buffer { get; }

    public SecureByteArray(int length) {
        Buffer = new byte[length];
        bufferHandle = GCHandle.Alloc(Buffer, GCHandleType.Pinned);
    }

    public void Dispose() {
        Zero();
        bufferHandle.Free();
    }

    [MethodImpl(MethodImplOptions.NoOptimization)]
    private void Zero() {
        for (int i = 0; i < Buffer.Length; i++) {
            Buffer[i] = 0;
        }
    }
}
