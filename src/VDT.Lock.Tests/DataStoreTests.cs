using System;
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
    public void Constructor() {
        var identity = new DataIdentity();
        var plainNameSpan = new ReadOnlySpan<byte>([110, 97, 109, 101]);
        using var subject = new DataStore(identity, plainNameSpan);

        Assert.Same(identity, subject.Identity);
        Assert.Equal(plainNameSpan, subject.Name);
    }

    [Fact]
    public void SetName() {
        using var subject = new DataStore();

        var plainPreviousValueBuffer = subject.GetBuffer("plainNameBuffer");

        subject.Name = new ReadOnlySpan<byte>([110, 97, 109, 101]);

        Assert.Equal(new ReadOnlySpan<byte>([110, 97, 109, 101]), subject.Name);
        Assert.True(plainPreviousValueBuffer.IsDisposed);
    }

    [Fact]
    public void SetItems() {
        using var subject = new DataStore();

        var previousItems = subject.Items;
        var newItems = new DataCollection<DataItem>();

        subject.Items = newItems;

        Assert.Same(newItems, subject.Items);
        Assert.True(previousItems.IsDisposed);
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
        DataIdentity identity;
        SecureBuffer plainNameBuffer;
        DataCollection<DataItem> items;

        using (var subject = new DataStore()) {
            identity = subject.Identity;
            plainNameBuffer = subject.GetBuffer("plainNameBuffer");
            items = subject.Items;
        }

        Assert.True(identity.IsDisposed);
        Assert.True(plainNameBuffer.IsDisposed);
        Assert.True(items.IsDisposed);
    }

    [Fact]
    public void IdentityThrowsIfDisposed() {
        DataStore disposedSubject;

        using (var subject = new DataStore()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => { var _ = disposedSubject.Identity; });
    }

    [Fact]
    public void GetNameThrowsIfDisposed() {
        DataStore disposedSubject;

        using (var subject = new DataStore()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => { var _ = disposedSubject.Name; });
    }

    [Fact]
    public void SetNameThrowsIfDisposed() {
        DataStore disposedSubject;

        using (var subject = new DataStore()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Name = new ReadOnlySpan<byte>([110, 97, 109, 101]));
    }

    [Fact]
    public void GetItemsThrowsIfDisposed() {
        DataStore disposedSubject;

        using (var subject = new DataStore()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Items);
    }

    [Fact]
    public void SetItemsThrowsIfDisposed() {
        DataStore disposedSubject;

        using (var subject = new DataStore()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Items = []);
    }

    [Fact]
    public void FieldLengthsThrowsIfDisposed() {
        DataStore disposedSubject;

        using (var subject = new DataStore()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.FieldLengths);
    }

    [Fact]
    public void SerializeToThrowsIfDisposed() {
        DataStore disposedSubject;
        using var plainBytes = new SecureByteList();

        using (var subject = new DataStore()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.SerializeTo(plainBytes));
    }
}
