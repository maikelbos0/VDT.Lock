using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace VDT.Lock.Tests;

public class DataCollectionTests {
    [Fact]
    public void Merge() {
        var expectedValue1 = new DataValue(DataProvider.CreateIdentity(1, 10), [115, 101, 108, 101, 99, 116, 111, 114]);
        var expectedValue2 = new DataValue(DataProvider.CreateIdentity(2, 10), [115, 101, 108, 101, 99, 116, 111, 114]);
        var expectedValue3 = new DataValue(DataProvider.CreateIdentity(3, 10), [115, 101, 108, 101, 99, 116, 111, 114]);

        var candidates = new List<DataCollection<DataValue>>() {
            new() {
                expectedValue2,
                new DataValue(DataProvider.CreateIdentity(3, 5), [111, 108, 100, 101, 114])
            },
            new() {
                expectedValue1,
                new DataValue(DataProvider.CreateIdentity(2, 5), [111, 108, 100, 101, 114])
            },
            new() {
                new DataValue(DataProvider.CreateIdentity(1, 5), [111, 108, 100, 101, 114]),
                expectedValue3
            }
        };

        var result = DataCollection.Merge(candidates);

        Assert.Equal([expectedValue1, expectedValue2, expectedValue3], result.OrderBy(selector => selector.Identity.Key[0]));

        foreach (var candidate in candidates) {
            Assert.True(candidate.IsDisposed);
        }
    }

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
    public void UnsafeClear() {
        using var subject = new DataCollection<DataValue>();
        var item = new DataValue();
        subject.Add(item);

        var result = subject.UnsafeClear();

        Assert.Empty(subject);
        Assert.Equal(item, Assert.Single(result));
        Assert.False(item.IsDisposed);
    }

    [Fact]
    public void SerializeToIncludingLength() {
        using var subject = new DataCollection<DataValue>() {
            DataProvider.CreateValue(0, [122, 101, 114, 111]),
            DataProvider.CreateValue(1, [111, 110, 101])
        };

        using var result = new SecureByteList();
        subject.SerializeTo(result, true);

        Assert.Equal(new ReadOnlySpan<byte>([95, 0, 0, 0, .. DataProvider.CreateSerializedValue(0, [122, 101, 114, 111]), .. DataProvider.CreateSerializedValue(1, [111, 110, 101])]), result.GetValue());
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
    public void CopyTo() {
        using var subject = new DataCollection<DataValue>() {
            DataProvider.CreateValue(0, [122, 101, 114, 111]),
            DataProvider.CreateValue(1, [111, 110, 101])
        };
        var target = new DataValue[3];

        subject.CopyTo(target, 1);

        Assert.Equal(subject.First(), target[1]);
        Assert.Equal(subject.Last(), target[2]);
    }

    [Fact]
    public void Dispose() {
        DataCollection<DataValue> subject;
        var item = new DataValue();

        using (subject = []) {
            subject.Add(item);
        }

        Assert.True(subject.IsDisposed);
        Assert.True(item.IsDisposed);
    }

    [Fact]
    public void CountThrowsIfDisposed() {
        DataCollection<DataValue> subject;

        using (subject = []) { }

        Assert.Throws<ObjectDisposedException>(() => subject.Count);
    }

    [Fact]
    public void IsReadOnlyThrowsIfDisposed() {
        DataCollection<DataValue> subject;

        using (subject = []) { }

        Assert.Throws<ObjectDisposedException>(() => subject.IsReadOnly);
    }

    [Fact]
    public void FieldLengthsThrowsIfDisposed() {
        DataCollection<DataValue> subject;

        using (subject = []) { }

        Assert.Throws<ObjectDisposedException>(() => subject.FieldLengths);
    }

    [Fact]
    public void AddThrowsIfDisposed() {
        DataCollection<DataValue> subject;
        using var item = new DataValue();

        using (subject = []) { }

        Assert.Throws<ObjectDisposedException>(() => subject.Add(item));
    }

    [Fact]
    public void ContainsThrowsIfDisposed() {
        DataCollection<DataValue> subject;

        using (subject = []) { }

        Assert.Throws<ObjectDisposedException>(() => subject.Contains(new DataValue()));
    }

    [Fact]
    public void RemoveThrowsIfDisposed() {
        DataCollection<DataValue> subject;

        using (subject = []) { }

        Assert.Throws<ObjectDisposedException>(() => subject.Remove(new DataValue()));
    }

    [Fact]
    public void ClearThrowsIfDisposed() {
        DataCollection<DataValue> subject;

        using (subject = []) { }

        Assert.Throws<ObjectDisposedException>(() => subject.Clear());
    }

    [Fact]
    public void UnsafeClearThrowsIfDisposed() {
        DataCollection<DataValue> subject;

        using (subject = []) { }

        Assert.Throws<ObjectDisposedException>(() => subject.UnsafeClear());
        }

    [Fact]
    public void CopyToThrowsIfDisposed() {
        DataCollection<DataValue> subject;

        using (subject = []) { }

        Assert.Throws<ObjectDisposedException>(() => subject.CopyTo([], 0));
    }

    [Fact]
    public void GetEnumeratorThrowsIfDisposed() {
        DataCollection<DataValue> subject;

        using (subject = []) { }

        Assert.Throws<ObjectDisposedException>(() => subject.GetEnumerator());
    }

    [Fact]
    public void SerializeToThrowsIfDisposed() {
        DataCollection<DataValue> subject;
        using var plainBytes = new SecureByteList();

        using (subject = []) { }

        Assert.Throws<ObjectDisposedException>(() => subject.SerializeTo(plainBytes));
    }
}
