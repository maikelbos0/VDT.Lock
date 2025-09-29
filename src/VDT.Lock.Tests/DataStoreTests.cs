using System;
using System.Collections.Generic;
using Xunit;

namespace VDT.Lock.Tests;

public class DataStoreTests {
    [Fact]
    public void DeserializeFrom() {
        var plainSpan = new ReadOnlySpan<byte>([.. DataProvider.CreateSerializedIdentity(0), 4, 0, 0, 0, 110, 97, 109, 101, 60, 0, 0, 0, .. DataProvider.CreateSerializedItem(1, [105, 116, 101, 109])]);

        using var subject = DataStore.DeserializeFrom(plainSpan);

        Assert.Equal(DataProvider.CreateIdentity(0), subject.Identity);
        Assert.Equal(new ReadOnlySpan<byte>([110, 97, 109, 101]), subject.Name);
        Assert.Equal(new ReadOnlySpan<byte>([105, 116, 101, 109]), Assert.Single(subject.Items).Name);
    }

    [Fact]
    public void SelectNewest() {
        var expectedItem = new DataItem(DataProvider.CreateIdentity(1, 10), [105, 116, 101, 109]);
        var expectedResult = new DataStore(DataProvider.CreateIdentity(0, 5), [110, 97, 109, 101]) {
            Items = {
                new(DataProvider.CreateIdentity(1, 5), [111, 108, 100, 101, 114]),
            }
        };

        var candidates = new List<DataStore>() {
            new(DataProvider.CreateIdentity(0, 3), [111, 108, 100, 101, 114]) {
                Items = {
                    new DataItem(DataProvider.CreateIdentity(1, 5), [111, 108, 100, 101, 114])
                }
            },
            expectedResult,
            new(DataProvider.CreateIdentity(0, 4), [111, 108, 100, 101, 114]) {
                Items = {
                    expectedItem
                }
            }
        };

        var result = DataStore.Merge(candidates);

        Assert.Same(expectedResult, result);
        Assert.Equal(expectedItem, Assert.Single(result.Items));

        foreach (var candidate in candidates) {
            Assert.Equal(candidate != expectedResult, candidate.IsDisposed);
        }
    }

    [Fact]
    public void Constructor() {
        var identity = new DataIdentity();
        var plainNameSpan = new ReadOnlySpan<byte>([110, 97, 109, 101]);
        using var subject = new DataStore(identity, plainNameSpan);

        Assert.Same(identity, subject.Identity);
        Assert.Equal(plainNameSpan, subject.Name);
    }

    [Fact]
    public void SetName() {
        using var subject = new DataStore(DataProvider.CreateIdentity(0, 0), []);
        var previousVersion = subject.Identity.Version;

        var plainPreviousValueBuffer = subject.GetBuffer("plainNameBuffer");

        subject.Name = new ReadOnlySpan<byte>([110, 97, 109, 101]);

        Assert.Equal(new ReadOnlySpan<byte>([110, 97, 109, 101]), subject.Name);
        Assert.True(plainPreviousValueBuffer.IsDisposed);
        Assert.False(previousVersion.SequenceEqual(subject.Identity.Version));
    }

    [Fact]
    public void SetItems() {
        using var subject = new DataStore(DataProvider.CreateIdentity(0, 0), []);
        var previousVersion = subject.Identity.Version;

        var previousItems = subject.Items;
        var newItems = new DataCollection<DataItem>();

        subject.Items = newItems;

        Assert.Same(newItems, subject.Items);
        Assert.True(previousItems.IsDisposed);
        Assert.False(previousVersion.SequenceEqual(subject.Identity.Version));
    }

    [Fact]
    public void FieldLengths() {
        using var subject = new DataStore([110, 97, 109, 101]);
        subject.Items.Add(DataProvider.CreateItem(0, [105, 116, 101, 109]));

        Assert.Equal([32, 4, 60], subject.FieldLengths);
    }

    [Fact]
    public void SerializeTo() {
        using var subject = new DataStore(DataProvider.CreateIdentity(0), [110, 97, 109, 101]);
        subject.Items.Add(DataProvider.CreateItem(1, [105, 116, 101, 109]));

        using var result = new SecureByteList();
        subject.SerializeTo(result);

        Assert.Equal(new ReadOnlySpan<byte>([.. DataProvider.CreateSerializedIdentity(0), 4, 0, 0, 0, 110, 97, 109, 101, 60, 0, 0, 0, .. DataProvider.CreateSerializedItem(1, [105, 116, 101, 109])]), result.GetValue());
    }

    [Fact]
    public void Dispose() {
        DataStore subject;
        DataIdentity identity;
        SecureBuffer plainNameBuffer;
        DataCollection<DataItem> items;

        using (subject = new()) {
            identity = subject.Identity;
            plainNameBuffer = subject.GetBuffer("plainNameBuffer");
            items = subject.Items;
        }

        Assert.True(subject.IsDisposed);
        Assert.True(identity.IsDisposed);
        Assert.True(plainNameBuffer.IsDisposed);
        Assert.True(items.IsDisposed);
    }

    [Fact]
    public void IdentityThrowsIfDisposed() {
        DataStore subject;

        using (subject = new()) { }

        Assert.Throws<ObjectDisposedException>(() => { var _ = subject.Identity; });
    }

    [Fact]
    public void GetNameThrowsIfDisposed() {
        DataStore subject;

        using (subject = new()) { }

        Assert.Throws<ObjectDisposedException>(() => { var _ = subject.Name; });
    }

    [Fact]
    public void SetNameThrowsIfDisposed() {
        DataStore subject;

        using (subject = new()) { }

        Assert.Throws<ObjectDisposedException>(() => subject.Name = new ReadOnlySpan<byte>([110, 97, 109, 101]));
    }

    [Fact]
    public void GetItemsThrowsIfDisposed() {
        DataStore subject;

        using (subject = new()) { }

        Assert.Throws<ObjectDisposedException>(() => subject.Items);
    }

    [Fact]
    public void SetItemsThrowsIfDisposed() {
        DataStore subject;

        using (subject = new()) { }

        Assert.Throws<ObjectDisposedException>(() => subject.Items = []);
    }

    [Fact]
    public void FieldLengthsThrowsIfDisposed() {
        DataStore subject;

        using (subject = new()) { }

        Assert.Throws<ObjectDisposedException>(() => subject.FieldLengths);
    }

    [Fact]
    public void SerializeToThrowsIfDisposed() {
        DataStore subject;
        using var plainBytes = new SecureByteList();

        using (subject = new()) { }

        Assert.Throws<ObjectDisposedException>(() => subject.SerializeTo(plainBytes));
    }
}
