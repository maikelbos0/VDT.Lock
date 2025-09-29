using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VDT.Lock;

public static class DataCollection {
    public static DataCollection<T> Merge<T>(IEnumerable<DataCollection<T>> candidates) where T : notnull, IData<T>, IIdentifiableData<T>, IDisposable {
        var candidateItems = new Dictionary<DataIdentity, List<T>>();
        var results = new DataCollection<T>();

        foreach (var candidate in candidates) {
            foreach (var selectorCandidate in candidate.UnsafeClear()) {
                if (candidateItems.TryGetValue(selectorCandidate.Identity, out var selectorCandidates)) {
                    selectorCandidates.Add(selectorCandidate);
                }
                else {
                    candidateItems.Add(selectorCandidate.Identity, [selectorCandidate]);
                }
            }

            candidate.Dispose();
        }

        foreach (var selectorCandidates in candidateItems.Values) {
            results.Add(T.Merge(selectorCandidates));
        }

        return results;
    }
}

public sealed class DataCollection<T> : IData<DataCollection<T>>, ICollection<T>, IEnumerable<T>, IDisposable where T : notnull, IData<T>, IDisposable {
    public static DataCollection<T> DeserializeFrom(ReadOnlySpan<byte> plainSpan) {
        var position = 0;
        var collection = new DataCollection<T>();

        while (position < plainSpan.Length) {
            collection.Add(T.DeserializeFrom(plainSpan.ReadSpan(ref position)));
        }

        return collection;
    }

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

    public IEnumerable<int> FieldLengths {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return items.Select(static item => item.GetLength());
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

    public List<T> UnsafeClear() {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        var clearedItems = new List<T>(items);

        items.Clear();

        return clearedItems;
    }

    public IEnumerator<T> GetEnumerator() {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        return items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void SerializeTo(SecureByteList plainBytes) => SerializeTo(plainBytes, true);

    public void SerializeTo(SecureByteList plainBytes, bool includeLength) {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        if (includeLength) {
            plainBytes.WriteInt(this.GetLength());
        }

        foreach (var item in items) {
            item.SerializeTo(plainBytes);
        }
    }

    public void CopyTo(T[] array, int arrayIndex) {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        items.CopyTo(array, arrayIndex);
    }

    public void Dispose() {
        foreach (var item in items) {
            item.Dispose();
        }
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }
}
