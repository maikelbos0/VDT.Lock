using System;
using Xunit;

namespace VDT.Lock.Tests;

public class DataFieldTests {
    [Fact]
    public void DeserializeFrom() {
        var plainSpan = new ReadOnlySpan<byte>([3, 0, 0, 0, 98, 97, 114, 5, 0, 0, 0, 5, 6, 7, 8, 9]);

        using var subject = DataField.DeserializeFrom(plainSpan);

        Assert.Equal(new ReadOnlySpan<byte>([98, 97, 114]), subject.Name);
        Assert.Equal(new ReadOnlySpan<byte>([5, 6, 7, 8, 9]), subject.Value);
    }

    [Fact]
    public void Constructor() {
        var plainNameSpan = new ReadOnlySpan<byte>([98, 97, 114]);
        var plainDataSpan = new ReadOnlySpan<byte>([5, 6, 7, 8, 9]);

        using var subject = new DataField(plainNameSpan, plainDataSpan);

        Assert.Equal(plainNameSpan, subject.Name);
        Assert.Equal(plainDataSpan, subject.Value);

    }

    [Fact]
    public void SetName() {
        using var subject = new DataField();

        var plainPreviousValueBuffer = subject.GetBuffer("plainNameBuffer");

        subject.Name = new ReadOnlySpan<byte>([99, 99, 99]);

        Assert.Equal(new ReadOnlySpan<byte>([99, 99, 99]), subject.Name);
        Assert.True(plainPreviousValueBuffer.IsDisposed);
    }
    
    [Fact]
    public void SetValue() {
        using var subject = new DataField();

        var plainPreviousValueBuffer = subject.GetBuffer("plainValueBuffer");

        subject.Value = new ReadOnlySpan<byte>([99, 99, 99]);

        Assert.Equal(new ReadOnlySpan<byte>([99, 99, 99]), subject.Value);
        Assert.True(plainPreviousValueBuffer.IsDisposed);
    }

    [Fact]
    public void Length() {
        using var subject = new DataField([98, 97, 114], [5, 6, 7, 8, 9]);

        Assert.Equal(16, subject.Length);
    }

    [Fact]
    public void SerializeTo() {
        using var subject = new DataField([98, 97, 114], [5, 6, 7, 8, 9]);

        using var result = new SecureByteList();
        subject.SerializeTo(result);

        Assert.Equal(new ReadOnlySpan<byte>([16, 0, 0, 0, 3, 0, 0, 0, 98, 97, 114, 5, 0, 0, 0, 5, 6, 7, 8, 9]), result.GetValue());
    }        

    [Fact]
    public void Dispose() {
        SecureBuffer plainNameBuffer;
        SecureBuffer plainDataBuffer;

        using (var subject = new DataField()) {
            plainNameBuffer = subject.GetBuffer("plainNameBuffer");
            plainDataBuffer = subject.GetBuffer("plainValueBuffer");
        };

        Assert.True(plainNameBuffer.IsDisposed);
        Assert.True(plainDataBuffer.IsDisposed);
    }

    [Fact]
    public void IsDisposed() {
        DataField disposedSubject;

        using (var subject = new DataField()) {
            disposedSubject = subject;
        };

        Assert.True(disposedSubject.IsDisposed);
    }

    [Fact]
    public void GetNameThrowsIfDisposed() {
        DataField disposedSubject;

        using (var subject = new DataField()) {
            disposedSubject = subject;
        };

        Assert.Throws<ObjectDisposedException>(() => { var _ = disposedSubject.Name; });
    }

    [Fact]
    public void SetNameThrowsIfDisposed() {
        DataField disposedSubject;

        using (var subject = new DataField()) {
            disposedSubject = subject;
        };

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Name = new ReadOnlySpan<byte>([15, 15, 15]));
    }

    [Fact]
    public void GetValueThrowsIfDisposed() {
        DataField disposedSubject;

        using (var subject = new DataField()) {
            disposedSubject = subject;
        };

        Assert.Throws<ObjectDisposedException>(() => { var _ = disposedSubject.Value; });
    }

    [Fact]
    public void SetValueThrowsIfDisposed() {
        DataField disposedSubject;

        using (var subject = new DataField()) {
            disposedSubject = subject;
        };

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Value = new ReadOnlySpan<byte>([15, 15, 15]));
    }

    [Fact]
    public void LengthThrowsIfDisposed() {
        DataField disposedSubject;

        using (var subject = new DataField()) {
            disposedSubject = subject;
        };

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Length);
    }

    [Fact]
    public void SerializeToThrowsIfDisposed() {
        DataField disposedSubject;
        using var plainBytes = new SecureByteList();

        using (var subject = new DataField()) {
            disposedSubject = subject;
        };

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.SerializeTo(plainBytes));
    }
}
