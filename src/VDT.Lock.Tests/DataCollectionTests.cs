using System;
using Xunit;

namespace VDT.Lock.Tests;

public class DataCollectionTests {
    public class TestDataItem : IData, IDisposable {
        public bool IsDisposed { get; private set; }

        public int Length => 4;

        public void Dispose() {
            IsDisposed = true;
            GC.SuppressFinalize(this);
        }

        public void SerializeTo(SecureByteList plainBytes) {
            plainBytes.WriteSpan([15, 0, 0, 0]);
        }
    }

    [Fact]
    public void Count() {
        using var subject = new DataCollection<TestDataItem> {
            new()
        };

#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.
        Assert.Equal(1, subject.Count);
#pragma warning restore xUnit2013 // Do not use equality check to check for collection size.
    }

    [Fact]
    public void IsReadOnly() {
        using var subject = new DataCollection<TestDataItem>();

        Assert.False(subject.IsReadOnly);
    }

    [Fact]
    public void Length() {
        using var subject = new DataCollection<TestDataItem>() {
            new(),
            new()
        };

        Assert.Equal(16, subject.Length);
    }

    [Fact]
    public void Add() {
        using var subject = new DataCollection<TestDataItem>();
        var item = new TestDataItem();

        subject.Add(item);

        Assert.Equal(item, Assert.Single(subject));
    }

    [Fact]
    public void ContainsIsTrueWhenPresent() {
        using var subject = new DataCollection<TestDataItem>();
        var item = new TestDataItem();

        subject.Add(item);

#pragma warning disable xUnit2017 // Do not use Contains() to check if a value exists in a collection
        Assert.True(subject.Contains(item));
#pragma warning restore xUnit2017 // Do not use Contains() to check if a value exists in a collection
    }

    [Fact]
    public void ContainsIsFalseWhenNotPresent() {
        using var subject = new DataCollection<TestDataItem>();

        using var item = new TestDataItem();

#pragma warning disable xUnit2017 // Do not use Contains() to check if a value exists in a collection
        Assert.False(subject.Contains(item));
#pragma warning restore xUnit2017 // Do not use Contains() to check if a value exists in a collection
    }

    [Fact]
    public void RemoveRemovesWhenPresent() {
        using var subject = new DataCollection<TestDataItem>();
        var item = new TestDataItem();

        subject.Add(item);

        Assert.True(subject.Remove(item));
        Assert.Empty(subject);
        Assert.True(item.IsDisposed);
    }

    [Fact]
    public void RemoveDoesNotRemoveWhenNotPresent() {
        using var subject = new DataCollection<TestDataItem>();

        using var item = new TestDataItem();

        Assert.False(subject.Remove(item));
        Assert.False(item.IsDisposed);
    }

    [Fact]
    public void Clear() {
        using var subject = new DataCollection<TestDataItem>();

        var item = new TestDataItem();
        subject.Add(item);

        subject.Clear();

        Assert.Empty(subject);
        Assert.True(item.IsDisposed);
    }

    [Fact]
    public void SerializeTo() {
        using var subject = new DataCollection<TestDataItem>() {
            new(),
            new()
        };

        using var result = new SecureByteList();
        subject.SerializeTo(result);

        Assert.Equal(new ReadOnlySpan<byte>([16, 0, 0, 0, 4, 0, 0, 0, 15, 0, 0, 0, 4, 0, 0, 0, 15, 0, 0, 0]), result.GetValue());
    }

    [Fact]
    public void CopyToThrows() {
        using var subject = new DataCollection<TestDataItem>();

        Assert.Throws<NotSupportedException>(() => subject.CopyTo([], 0));
    }

    [Fact]
    public void Dispose() {
        var item = new TestDataItem();

        using (var subject = new DataCollection<TestDataItem>()) {
            subject.Add(item);
        };

        Assert.True(item.IsDisposed);
    }

    [Fact]
    public void IsDisposed() {
        DataCollection<TestDataItem> disposedSubject;

        using (var subject = new DataCollection<TestDataItem>()) {
            disposedSubject = subject;
        };

        Assert.True(disposedSubject.IsDisposed);
    }

    [Fact]
    public void CountThrowsIfDisposed() {
        DataCollection<TestDataItem> disposedSubject;

        using (var subject = new DataCollection<TestDataItem>()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Count);
    }

    [Fact]
    public void IsReadOnlyThrowsIfDisposed() {
        DataCollection<TestDataItem> disposedSubject;

        using (var subject = new DataCollection<TestDataItem>()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.IsReadOnly);
    }

    [Fact]
    public void LengthThrowsIfDisposed() {
        DataCollection<TestDataItem> disposedSubject;

        using (var subject = new DataCollection<TestDataItem>()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Length);
    }

    [Fact]
    public void AddThrowsIfDisposed() {
        DataCollection<TestDataItem> disposedSubject;
        using var item = new TestDataItem();

        using (var subject = new DataCollection<TestDataItem>()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Add(item));
    }

    [Fact]
    public void ContainsThrowsIfDisposed() {
        DataCollection<TestDataItem> disposedSubject;

        using (var subject = new DataCollection<TestDataItem>()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Contains(new TestDataItem()));
    }

    [Fact]
    public void RemoveThrowsIfDisposed() {
        DataCollection<TestDataItem> disposedSubject;

        using (var subject = new DataCollection<TestDataItem>()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Remove(new TestDataItem()));
    }

    [Fact]
    public void ClearThrowsIfDisposed() {
        DataCollection<TestDataItem> disposedSubject;

        using (var subject = new DataCollection<TestDataItem>()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Clear());
    }

    [Fact]
    public void GetEnumeratorThrowsIfDisposed() {
        DataCollection<TestDataItem> disposedSubject;

        using (var subject = new DataCollection<TestDataItem>()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.GetEnumerator());
    }

    [Fact]
    public void SerializeToThrowsIfDisposed() {
        DataCollection<TestDataItem> disposedSubject;
        using var plainBytes = new SecureByteList();

        using (var subject = new DataCollection<TestDataItem>()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.SerializeTo(plainBytes));
    }
}
