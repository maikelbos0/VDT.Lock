using System;
using System.Linq;
using Xunit;

namespace VDT.Lock.Tests;

public class DataCollectionTests {
    [Fact]
    public void DeserializeFrom() {
        var plainSpan = new ReadOnlySpan<byte>([.. DataProvider.CreateSerializedValue(0, [122, 101, 114, 111]), .. DataProvider.CreateSerializedValue(1, [111, 110, 101])]);

        using var subject = DataCollection<DataValue>.DeserializeFrom(plainSpan);

        Assert.Equal(2, subject.Count);
        Assert.Contains(subject, dataValue => dataValue.Value.SequenceEqual(new byte[] { 122, 101, 114, 111 }));
        Assert.Contains(subject, dataValue => dataValue.Value.SequenceEqual(new byte[] { 111, 110, 101 }));
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
            new([122, 101, 114, 111]),
            new([111, 110, 101])
        };

        Assert.Equal([44, 43], subject.FieldLengths);
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

    [Fact]
    public void SerializeToIncludingLength() {
        using var subject = new DataCollection<DataValue>() {
            DataProvider.CreateValue(0, [122, 101, 114, 111]),
            DataProvider.CreateValue(1, [111, 110, 101])
        };

        using var result = new SecureByteList();
        subject.SerializeTo(result, true);

        Assert.Equal(new ReadOnlySpan<byte>([95, 0, 0, 0, ..DataProvider.CreateSerializedValue(0, [122, 101, 114, 111]), ..DataProvider.CreateSerializedValue(1, [111, 110, 101])]), result.GetValue());
    }

    [Fact]
    public void SerializeToExcludingLength() {
        using var subject = new DataCollection<DataValue>() {
            DataProvider.CreateValue(0, [122, 101, 114, 111]),
            DataProvider.CreateValue(1, [111, 110, 101])
        };

        using var result = new SecureByteList();
        subject.SerializeTo(result, false);

        Assert.Equal(new ReadOnlySpan<byte>([.. DataProvider.CreateSerializedValue(0, [122, 101, 114, 111]), .. DataProvider.CreateSerializedValue(1, [111, 110, 101])]), result.GetValue());
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
