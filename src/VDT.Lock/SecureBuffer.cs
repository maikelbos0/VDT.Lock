using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace VDT.Lock;

// TODO: while instances of this class and its implementations may be short-lived, ideally it should still be prevented from being swapped to disk
internal sealed class SecureBuffer : IDisposable {
    private GCHandle handle;

    public byte[] Value { get; }

    public SecureBuffer(byte[] buffer) {
        Value = buffer;
        handle = GCHandle.Alloc(Value, GCHandleType.Pinned);
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~SecureBuffer() {
        Dispose(false);
    }

    private void Dispose(bool _) {
        CryptographicOperations.ZeroMemory(Value);
        handle.Free();
    }
}
