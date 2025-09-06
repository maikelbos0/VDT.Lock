using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace VDT.Lock;

// TODO while instances of this class and its implementations may be short-lived, ideally it should still be prevented from being swapped to disk
public sealed class SecureBuffer : IDisposable {
    private readonly GCHandle handle;
    private readonly byte[] value;

    public bool IsDisposed { get; private set; }

    public byte[] Value {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return value;
        }
    }

    // TOOD figure out if it is a good idea to have this
    public int Length => value.Length;

    public SecureBuffer(int size) : this(new byte[size]) { }

    public SecureBuffer(byte[] buffer) {
        value = buffer;
        handle = GCHandle.Alloc(Value, GCHandleType.Pinned);
    }

    // TOOD figure out if it is a good idea to have this
    public void SerializeTo(SecureByteList plainBytes) {
        plainBytes.WriteInt(Length);
        plainBytes.Add(value);
    }

    public void Dispose() {
        Dispose(true);
        IsDisposed = true;
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
    }
}
