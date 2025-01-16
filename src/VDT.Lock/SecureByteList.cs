using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace VDT.Lock;

public sealed class SecureByteList : IDisposable {
    public const int DefaultCapacity = 64;

    public static int GetCapacity(int requestedCapacity) {
        var capacity = DefaultCapacity;

        while (capacity < requestedCapacity) {
            capacity *= 2;
        }

        return Math.Min(capacity, Array.MaxLength);
    }

    private SecureBuffer buffer;
    private int length = 0;

    public bool IsDisposed { get; private set; }

    // TODO add indexer?

    public SecureByteList() {
        buffer = new(DefaultCapacity);
    }

    public SecureByteList(Stream stream) {
        int bytesRead;

        buffer = new(GetCapacity(stream.CanSeek ? (int)stream.Length + DefaultCapacity : DefaultCapacity));

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

    public void Add(char c) => Add(Encoding.UTF8.GetBytes([c]));

    public void Add(byte b) {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        EnsureCapacity(length + 1);
        buffer.Value[length++] = b;
    }

    public void Add(ReadOnlySpan<byte> span) {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        EnsureCapacity(length + span.Length);
        span.CopyTo(new Span<byte>(buffer.Value, length, span.Length));
        length += span.Length;
    }

    public void EnsureCapacity(int requestedCapacity) {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        if (buffer.Value.Length < requestedCapacity && buffer.Value.Length < Array.MaxLength) {
            var capacity = GetCapacity(requestedCapacity);
            var newBytes = new byte[capacity];

            Buffer.BlockCopy(buffer.Value, 0, newBytes, 0, buffer.Value.Length);
            buffer.Dispose();
            buffer = new(newBytes);
        }
    }

    public void RemoveLast() {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        CryptographicOperations.ZeroMemory(new Span<byte>(buffer.Value, --length, 1));
    }

    public void Clear() {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        CryptographicOperations.ZeroMemory(buffer.Value);
        length = 0;
    }

    public ReadOnlySpan<byte> GetValue() {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        return new(buffer.Value, 0, length);
    }

    public SecureBuffer ToBuffer() {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        var bytes = new byte[length];

        Buffer.BlockCopy(buffer.Value, 0, bytes, 0, length);

        return new SecureBuffer(bytes);
    }

    public void Dispose() {
        buffer.Dispose();
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }
}
