using System;
using System.Collections.Generic;
using Xunit;

namespace VDT.Lock.Tests;

public class DataCollectionTests {
    public class TestDisposable : IDisposable {
        public bool IsDisposed { get; private set; }

        public void Dispose() {
            IsDisposed = true;
            GC.SuppressFinalize(this);
        }
    }

    [Fact]
    public void Count() {
        using var subject = new DataCollection<TestDisposable>();

        subject.Add();

        Assert.Equal(1, subject.Count);
    }

    [Fact]
    public void Add() {
        using var subject = new DataCollection<TestDisposable>();

        var item = subject.Add();

        Assert.Equal(item, Assert.Single(subject));
    }

    [Fact]
    public void ContainsWhenPresent() {
        using var subject = new DataCollection<TestDisposable>();

        var item = subject.Add();

        Assert.True(subject.Contains(item));
    }

    [Fact]
    public void ContainsWhenNotPresent() {
        using var subject = new DataCollection<TestDisposable>();

        using var item = new TestDisposable();

        Assert.False(subject.Contains(item));
    }

    [Fact]
    public void RemoveWhenPresent() {
        using var subject = new DataCollection<TestDisposable>();

        var item = subject.Add();

        Assert.True(subject.Remove(item));
        Assert.True(item.IsDisposed);
    }

    [Fact]
    public void RemoveWhenNotPresent() {
        using var subject = new DataCollection<TestDisposable>();

        using var item = new TestDisposable();

        Assert.False(subject.Remove(item));
        Assert.False(item.IsDisposed);
    }

    [Fact]
    public void Clear() {
        var items = new List<TestDisposable>();
        using var subject = new DataCollection<TestDisposable>();

        items.Add(subject.Add());
        items.Add(subject.Add());
        subject.Clear();

        Assert.Equal(0, subject.Count);
        Assert.Equal(2, items.Count);

        foreach (var item in items) {
            Assert.True(item.IsDisposed);
        }
    }

    [Fact]
    public void Dispose() {
        var items = new List<TestDisposable>();

        using (var subject = new DataCollection<TestDisposable>()) {
            items.Add(subject.Add());
            items.Add(subject.Add());
        };

        Assert.Equal(2, items.Count);

        foreach (var item in items) {
            Assert.True(item.IsDisposed);
        }
    }

    [Fact]
    public void IsDisposed() {
        DataCollection<TestDisposable> disposedSubject;

        using (var subject = new DataCollection<TestDisposable>()) {
            disposedSubject = subject;
        };

        Assert.True(disposedSubject.IsDisposed);
    }

    [Fact]
    public void CountThrowsIfDisposed() {
        DataCollection<TestDisposable> disposedSubject;

        using (var subject = new DataCollection<TestDisposable>()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Count);
    }

    [Fact]
    public void AddThrowsIfDisposed() {
        DataCollection<TestDisposable> disposedSubject;

        using (var subject = new DataCollection<TestDisposable>()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Add());
    }

    [Fact]
    public void ContainsThrowsIfDisposed() {
        DataCollection<TestDisposable> disposedSubject;

        using (var subject = new DataCollection<TestDisposable>()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Contains(new TestDisposable()));
    }

    [Fact]
    public void RemoveThrowsIfDisposed() {
        DataCollection<TestDisposable> disposedSubject;

        using (var subject = new DataCollection<TestDisposable>()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Remove(new TestDisposable()));
    }

    [Fact]
    public void ClearThrowsIfDisposed() {
        DataCollection<TestDisposable> disposedSubject;

        using (var subject = new DataCollection<TestDisposable>()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Clear());
    }

    [Fact]
    public void GetEnumeratorThrowsIfDisposed() {
        DataCollection<TestDisposable> disposedSubject;

        using (var subject = new DataCollection<TestDisposable>()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.GetEnumerator());
    }
}
