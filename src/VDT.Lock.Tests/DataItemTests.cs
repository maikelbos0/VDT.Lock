using System;
using Xunit;

namespace VDT.Lock.Tests;

public class DataItemTests {
    [Fact]
    public void Constructor() {
        using var subject = new DataItem([98, 97, 114]);

        Assert.Equal(new ReadOnlySpan<byte>([98, 97, 114]), subject.Name);
    }

    [Fact]
    public void SetName() {
        using var subject = new DataItem();

        var plainPreviousValueBuffer = subject.GetBuffer("plainNameBuffer");

        subject.Name = new ReadOnlySpan<byte>([99, 99, 99]);

        Assert.Equal(new ReadOnlySpan<byte>([99, 99, 99]), subject.Name);
        Assert.True(plainPreviousValueBuffer.IsDisposed);
    }

    [Fact]
    public void Dispose() {
        SecureBuffer plainNameBuffer;
        DataCollection<DataField> fields;

        using (var subject = new DataItem()) {
            plainNameBuffer = subject.GetBuffer("plainNameBuffer");
            fields = subject.Fields;
        }

        Assert.True(plainNameBuffer.IsDisposed);
        Assert.True(fields.IsDisposed);
    }


    [Fact]
    public void GetNameThrowsIfDisposed() {
        DataItem disposedSubject;

        using (var subject = new DataItem()) {
            disposedSubject = subject;
        };

        Assert.Throws<ObjectDisposedException>(() => { var _ = disposedSubject.Name; });
    }

    [Fact]
    public void SetNameThrowsIfDisposed() {
        DataItem disposedSubject;

        using (var subject = new DataItem()) {
            disposedSubject = subject;
        };

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Name = new ReadOnlySpan<byte>([15, 15, 15]));
    }

    [Fact]
    public void FieldsThrowsIfDisposed() {
        DataItem disposedSubject;

        using (var subject = new DataItem()) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Fields);
    }
}
