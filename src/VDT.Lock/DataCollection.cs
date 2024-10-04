using System;
using System.Collections;
using System.Collections.Generic;

namespace VDT.Lock;

public sealed class DataCollection<T> : IEnumerable<T>, IDisposable where T : notnull, IDisposable, new() {
    private readonly List<T> items = [];

    public int Count => items.Count;

    public T Add() {
        var item = new T();
        items.Add(item);
        return item;
    }

    public bool Contains(T item) => items.Contains(item);

    public bool Remove(T item) {
        var removed = items.Remove(item);

        if (removed) {
            item.Dispose();
        }

        return removed;
    }

    public void Clear() {
        foreach (var item in items) {
            item.Dispose();
        }
        items.Clear();
    }

    public IEnumerator<T> GetEnumerator() => items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => items.GetEnumerator();

    public void Dispose() {
        foreach (var item in items) {
            item.Dispose();
        }
        GC.SuppressFinalize(this);
    }
}
