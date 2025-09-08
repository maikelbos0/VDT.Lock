using System;
using System.Linq;
using Xunit;

namespace VDT.Lock.Tests;

public class DataCollectionTests {
    [Fact]
    public void DeserializeFrom() {
        var plainSpan = new ReadOnlySpan<byte>([7, 0, 0, 0, 3, 0, 0, 0, 102, 111, 111, 9, 0, 0, 0, 5, 0, 0, 0, 1, 2, 3, 4, 5]);

        using var subject = DataCollection<DataValue>.DeserializeFrom(plainSpan);

        Assert.Equal(2, subject.Count);
        Assert.Contains(subject, dataValue => dataValue.Value.SequenceEqual(new byte[] { 102, 111, 111 }));
        Assert.Contains(subject, dataValue => dataValue.Value.SequenceEqual(new byte[] { 1, 2, 3, 4, 5 }));
    }

    [Fact]
    public void Count() {
        using var subject = new DataCollection<DataValue> {
            new(),
            new()
        };

        Assert.Equal(2, subject.Count);
    }

    [Fact]
    public void IsReadOnly() {
        using var subject = new DataCollection<DataValue>();

        Assert.False(subject.IsReadOnly);
    }

    [Fact]
    public void FieldLengths() {
        using var subject = new DataCollection<DataValue>() {
            new([102, 111, 111]),
            new([1, 2, 3, 4, 5])
        };

        Assert.Equal([7, 9], subject.FieldLengths);
    }

    [Fact]
    public void Add() {
        using var subject = new DataCollection<DataValue>();
        var item = new DataValue();

        subject.Add(item);

        Assert.Equal(item, Assert.Single(subject));
    }

    [Fact]
    public void ContainsIsTrueWhenPresent() {
        using var subject = new DataCollection<DataValue>();
        var item = new DataValue();

        subject.Add(item);

#pragma warning disable xUnit2017 // Do not use Contains() to check if a value exists in a collection
        Assert.True(subject.Contains(item));
#pragma warning restore xUnit2017 // Do not use Contains() to check if a value exists in a collection
    }

    [Fact]
    public void ContainsIsFalseWhenNotPresent() {
        using var subject = new DataCollection<DataValue>();
        using var item = new DataValue();

#pragma warning disable xUnit2017 // Do not use Contains() to check if a value exists in a collection
        Assert.False(subject.Contains(item));
#pragma warning restore xUnit2017 // Do not use Contains() to check if a value exists in a collection
    }

    [Fact]
    public void RemoveRemovesWhenPresent() {
        using var subject = new DataCollection<DataValue>();
        var item = new DataValue();

        subject.Add(item);

        Assert.True(subject.Remove(item));
        Assert.Empty(subject);
        Assert.True(item.IsDisposed);
    }

    [Fact]
    public void RemoveDoesNotRemoveWhenNotPresent() {
        using var subject = new DataCollection<DataValue>();
        using var item = new DataValue();

        Assert.False(subject.Remove(item));
        Assert.False(item.IsDisposed);
    }

    [Fact]
    public void Clear() {
        using var subject = new DataCollection<DataValue>();
        var item = new DataValue();
        subject.Add(item);

        subject.Clear();

        Assert.Empty(subject);
        Assert.True(item.IsDisposed);
    }

    [Theory]
    [InlineData(true, new byte[] { 24, 0, 0, 0, 7, 0, 0, 0, 3, 0, 0, 0, 102, 111, 111, 9, 0, 0, 0, 5, 0, 0, 0, 1, 2, 3, 4, 5 })]
    [InlineData(false, new byte[] { 7, 0, 0, 0, 3, 0, 0, 0, 102, 111, 111, 9, 0, 0, 0, 5, 0, 0, 0, 1, 2, 3, 4, 5 })]
    public void SerializeTo(bool includeLength, byte[] expectedResult) {
        using var subject = new DataCollection<DataValue>() {
            new([102, 111, 111]),
            new([1, 2, 3, 4, 5])
        };

        using var result = new SecureByteList();
        subject.SerializeTo(result, includeLength);

        Assert.Equal(expectedResult, result.GetValue());
    }

    [Fact]
    public void CopyToThrows() {
        using var subject = new DataCollection<DataValue>();

        Assert.Throws<NotSupportedException>(() => subject.CopyTo([], 0));
    }

    [Fact]
    public void Dispose() {
        var item = new DataValue();

        using (var subject = new DataCollection<DataValue>()) {
            subject.Add(item);
        }

        Assert.True(item.IsDisposed);
    }

    [Fact]
    public void IsDisposed() {
        DataCollection<DataValue> disposedSubject;

        using (var subject = new DataCollection<DataValue>()) {
            disposedSubject = subject;
        }

        Assert.True(disposedSubject.IsDisposed);
    }

    [Fact]
    public void CountThrowsIfDisposed() {
        DataCollection<DataValue> disposedSubject;

        using (var subject = new DataCollection<DataValue>()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Count);
    }

    [Fact]
    public void IsReadOnlyThrowsIfDisposed() {
        DataCollection<DataValue> disposedSubject;

        using (var subject = new DataCollection<DataValue>()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.IsReadOnly);
    }

    [Fact]
    public void FieldLengthsThrowsIfDisposed() {
        DataCollection<DataValue> disposedSubject;

        using (var subject = new DataCollection<DataValue>()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.FieldLengths);
    }

    [Fact]
    public void AddThrowsIfDisposed() {
        DataCollection<DataValue> disposedSubject;
        using var item = new DataValue();

        using (var subject = new DataCollection<DataValue>()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Add(item));
    }

    [Fact]
    public void ContainsThrowsIfDisposed() {
        DataCollection<DataValue> disposedSubject;

        using (var subject = new DataCollection<DataValue>()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Contains(new DataValue()));
    }

    [Fact]
    public void RemoveThrowsIfDisposed() {
        DataCollection<DataValue> disposedSubject;

        using (var subject = new DataCollection<DataValue>()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Remove(new DataValue()));
    }

    [Fact]
    public void ClearThrowsIfDisposed() {
        DataCollection<DataValue> disposedSubject;

        using (var subject = new DataCollection<DataValue>()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Clear());
    }

    [Fact]
    public void GetEnumeratorThrowsIfDisposed() {
        DataCollection<DataValue> disposedSubject;

        using (var subject = new DataCollection<DataValue>()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.GetEnumerator());
    }

    [Fact]
    public void SerializeToThrowsIfDisposed() {
        DataCollection<DataValue> disposedSubject;
        using var plainBytes = new SecureByteList();

        using (var subject = new DataCollection<DataValue>()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.SerializeTo(plainBytes));
    }
}
