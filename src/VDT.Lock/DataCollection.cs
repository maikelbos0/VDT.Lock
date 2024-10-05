﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace VDT.Lock;

public sealed class DataCollection<T> : IEnumerable<T>, IDisposable where T : notnull, IDisposable, new() {
    private readonly List<T> items = [];

    public bool IsDisposed { get; private set; }

    public int Count {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return items.Count;
        }
    }

    public T Add() {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        var item = new T();
        items.Add(item);
        return item;
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

    public void Dispose() {
        foreach (var item in items) {
            item.Dispose();
        }
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }
}
