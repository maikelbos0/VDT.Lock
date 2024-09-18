using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace VDT.Lock;

// TODO: while instances of this class and its implementations may be short-lived, ideally it should still be prevented from being swapped to disk
public abstract class SecureByteCollectionBase : IDisposable {
    private protected byte[] buffer = [];
    private protected GCHandle bufferHandle;

    private protected void SetBuffer(byte[] buffer) {
        this.buffer = buffer;
        bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
    }

    private protected void ReleaseBuffer() {
        CryptographicOperations.ZeroMemory(buffer);
        bufferHandle.Free();
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~SecureByteCollectionBase() {
        Dispose(false);
    }

    private void Dispose(bool _) {
        ReleaseBuffer();
    }
}
