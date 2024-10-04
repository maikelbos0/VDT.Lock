using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace VDT.Lock;

// TODO: while instances of this class and its implementations may be short-lived, ideally it should still be prevented from being swapped to disk
public sealed class SecureBuffer : IDisposable {
    private readonly GCHandle handle;
    private readonly byte[] value;
    private bool isDisposed;

    public byte[] Value {
        get {
            ObjectDisposedException.ThrowIf(isDisposed, this);
            return value;
        }
    }

    public bool IsDisposed => isDisposed;
    
    public SecureBuffer(int size) : this(new byte[size]) { }

    public SecureBuffer(byte[] buffer) {
        value = buffer;
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
        CryptographicOperations.ZeroMemory(value);
        if (handle.IsAllocated) {
            handle.Free();
        }
        isDisposed = true;
    }
}
