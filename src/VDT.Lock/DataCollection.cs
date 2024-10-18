using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VDT.Lock;

public sealed class DataCollection<T> : IData, ICollection<T>, IEnumerable<T>, IDisposable where T : notnull, IData, IDisposable {
    private readonly List<T> items = [];

    public bool IsDisposed { get; private set; }

    public int Count {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return items.Count;
        }
    }

    public bool IsReadOnly {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return false;
        }
    }

    public int Length {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return items.Sum(static item => item.Length + 4);
        }
    }

    public void Add(T item) {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        items.Add(item);
    }

    public bool Contains(T item) {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        return items.Contains(item);
    }

    public bool Remove(T item) {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        var removed = items.Remove(item);

        if (removed) {
            item.Dispose();
        }

        return removed;
    }

    public void Clear() {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        foreach (var item in items) {
            item.Dispose();
        }

        items.Clear();
    }

    public IEnumerator<T> GetEnumerator() {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        return items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void SerializeTo(SecureByteList plainBytes) {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        plainBytes.WriteInt(Length);

        foreach (var item in items) {
            item.SerializeTo(plainBytes);
        }
    }

    public void CopyTo(T[] array, int arrayIndex) {
        throw new NotSupportedException();
    }

    public void Dispose() {
        foreach (var item in items) {
            item.Dispose();
        }
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }
}
