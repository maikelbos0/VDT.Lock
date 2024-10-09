using System;
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
    public void IsReadOnly() {
        using var subject = new DataCollection<TestDisposable>();

        Assert.False(subject.IsReadOnly);
    }

    [Fact]
    public void Count() {
        using var subject = new DataCollection<TestDisposable> {
            new()
        };

#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.
        Assert.Equal(1, subject.Count);
#pragma warning restore xUnit2013 // Do not use equality check to check for collection size.
    }

    [Fact]
    public void Add() {
        using var subject = new DataCollection<TestDisposable>();
        var item = new TestDisposable();

        subject.Add(item);

        Assert.Equal(item, Assert.Single(subject));
    }

    [Fact]
    public void ContainsIsTrueWhenPresent() {
        using var subject = new DataCollection<TestDisposable>();
        var item = new TestDisposable();

        subject.Add(item);

#pragma warning disable xUnit2017 // Do not use Contains() to check if a value exists in a collection
        Assert.True(subject.Contains(item));
#pragma warning restore xUnit2017 // Do not use Contains() to check if a value exists in a collection
    }

    [Fact]
    public void ContainsIsFalseWhenNotPresent() {
        using var subject = new DataCollection<TestDisposable>();

        using var item = new TestDisposable();

#pragma warning disable xUnit2017 // Do not use Contains() to check if a value exists in a collection
        Assert.False(subject.Contains(item));
#pragma warning restore xUnit2017 // Do not use Contains() to check if a value exists in a collection
    }

    [Fact]
    public void RemoveRemovesWhenPresent() {
        using var subject = new DataCollection<TestDisposable>();
        var item = new TestDisposable();

        subject.Add(item);

        Assert.True(subject.Remove(item));
        Assert.Empty(subject);
        Assert.True(item.IsDisposed);
    }

    [Fact]
    public void RemoveDoesNotRemoveWhenNotPresent() {
        using var subject = new DataCollection<TestDisposable>();

        using var item = new TestDisposable();

        Assert.False(subject.Remove(item));
        Assert.False(item.IsDisposed);
    }

    [Fact]
    public void Clear() {
        using var subject = new DataCollection<TestDisposable>();

        var item = new TestDisposable();
        subject.Add(item);

        subject.Clear();

        Assert.Empty(subject);
        Assert.True(item.IsDisposed);
    }

    [Fact]
    public void CopyToThrows() {
        using var subject = new DataCollection<TestDisposable>();

        Assert.Throws<NotSupportedException>(() => subject.CopyTo([], 0));
    }

    [Fact]
    public void Dispose() {
        var item = new TestDisposable();

        using (var subject = new DataCollection<TestDisposable>()) {
            subject.Add(item);
        };

        Assert.True(item.IsDisposed);
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
    public void IsReadOnlyThrowsIfDisposed() {
        DataCollection<TestDisposable> disposedSubject;

        using (var subject = new DataCollection<TestDisposable>()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.IsReadOnly);
    }

    [Fact]
    public void AddThrowsIfDisposed() {
        DataCollection<TestDisposable> disposedSubject;
        using var item = new TestDisposable();

        using (var subject = new DataCollection<TestDisposable>()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Add(item));
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
