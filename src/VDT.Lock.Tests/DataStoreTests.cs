using System;
using Xunit;

namespace VDT.Lock.Tests;

public class DataStoreTests {
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
    public void Length() {
        using var subject = new DataStore([98, 97, 114]);
        subject.Items.Add(new DataItem([102, 111, 111]));
        subject.Items.Add(new DataItem([5, 6, 7, 8, 9]));

        Assert.Equal(59, subject.Length);
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
        };

        Assert.Throws<ObjectDisposedException>(() => { var _ = disposedSubject.Name; });
    }

    [Fact]
    public void SetNameThrowsIfDisposed() {
        DataStore disposedSubject;

        using (var subject = new DataStore()) {
            disposedSubject = subject;
        };

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Name = new ReadOnlySpan<byte>([15, 15, 15]));
    }

    [Fact]
    public void ItemsThrowsIfDisposed() {
        DataStore disposedSubject;

        using (var subject = new DataStore()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Items);
    }

    [Fact]
    public void LengthThrowsIfDisposed() {
        DataStore disposedSubject;

        using (var subject = new DataStore()) {
            disposedSubject = subject;
        };

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Length);
    }
}
