using System;
using Xunit;

namespace VDT.Lock.Tests;

public class DataStoreTests {
    [Fact]
    public void DeserializeFromDeserializesName() {
        var plainSpan = new ReadOnlySpan<byte>([3, 0, 0, 0, 98, 97, 114, 0, 0, 0, 0]);

        using var subject = DataStore.DeserializeFrom(plainSpan);

        Assert.Equal(new ReadOnlySpan<byte>([98, 97, 114]), subject.Name);
    }

    [Fact]
    public void DeserializeFromDeserializesItems() {
        var plainSpan = new ReadOnlySpan<byte>([0, 0, 0, 0, 46, 0, 0, 0, 19, 0, 0, 0, 3, 0, 0, 0, 98, 97, 114, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 19, 0, 0, 0, 3, 0, 0, 0, 102, 111, 111, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]);

        using var subject = DataStore.DeserializeFrom(plainSpan);

        Assert.Equal(2, subject.Items.Count);
        Assert.Contains(subject.Items, item => item.Name.SequenceEqual(new ReadOnlySpan<byte>([98, 97, 114])));
        Assert.Contains(subject.Items, item => item.Name.SequenceEqual(new ReadOnlySpan<byte>([102, 111, 111])));
    }

    [Fact]
    public void Constructor() {
        using var subject = new DataStore([98, 97, 114]);

        Assert.Equal(new ReadOnlySpan<byte>([98, 97, 114]), subject.Name);
    }

    [Fact]
    public void SetName() {
        using var subject = new DataStore();

        var plainPreviousValueBuffer = subject.GetBuffer("plainNameBuffer");

        subject.Name = new ReadOnlySpan<byte>([99, 99, 99]);

        Assert.Equal(new ReadOnlySpan<byte>([99, 99, 99]), subject.Name);
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
        using var subject = new DataStore([98, 97, 114]);
        subject.Items.Add(new DataItem([102, 111, 111]));
        subject.Items.Add(new DataItem([5, 6, 7, 8, 9]));

        Assert.Equal([3, 48], subject.FieldLengths);
    }

    [Fact]
    public void SerializeTo() {
        using var subject = new DataStore([98, 97, 114]);
        subject.Items.Add(new DataItem([102, 111, 111]));
        subject.Items.Add(new DataItem([5, 6, 7, 8, 9]));

        using var result = new SecureByteList();
        subject.SerializeTo(result);

        Assert.Equal(new ReadOnlySpan<byte>([59, 0, 0, 0, 3, 0, 0, 0, 98, 97, 114, 48, 0, 0, 0, 19, 0, 0, 0, 3, 0, 0, 0, 102, 111, 111, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 21, 0, 0, 0, 5, 0, 0, 0, 5, 6, 7, 8, 9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]), result.GetValue());
    }

    [Fact]
    public void Dispose() {
        SecureBuffer plainNameBuffer;
        DataCollection<DataItem> items;

        using (var subject = new DataStore()) {
            plainNameBuffer = subject.GetBuffer("plainNameBuffer");
            items = subject.Items;
        }

        Assert.True(plainNameBuffer.IsDisposed);
        Assert.True(items.IsDisposed);
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

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Name = new ReadOnlySpan<byte>([15, 15, 15]));
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
