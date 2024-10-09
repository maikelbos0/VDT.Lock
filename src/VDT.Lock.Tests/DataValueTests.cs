using System;
using Xunit;

namespace VDT.Lock.Tests;

public class DataValueTests {
    [Fact]
    public void Constructor() {
        var plainValueSpan = new ReadOnlySpan<byte>([98, 97, 114]);

        using var subject = new DataValue(plainValueSpan);

        Assert.Equal(plainValueSpan, subject.Value);

    }

    [Fact]
    public void SetValue() {
        using var subject = new DataValue();

        var plainPreviousValueBuffer = subject.GetBuffer("plainValueBuffer");

        subject.Value = new ReadOnlySpan<byte>([99, 99, 99]);

        Assert.Equal(new ReadOnlySpan<byte>([99, 99, 99]), subject.Value);
        Assert.True(plainPreviousValueBuffer.IsDisposed);
    }
    
    [Fact]
    public void Dispose() {
        SecureBuffer plainValueBuffer;

        using (var subject = new DataValue()) {
            plainValueBuffer = subject.GetBuffer("plainValueBuffer");
        };

        Assert.True(plainValueBuffer.IsDisposed);
    }

    [Fact]
    public void IsDisposed() {
        DataValue disposedSubject;

        using (var subject = new DataValue()) {
            disposedSubject = subject;
        };

        Assert.True(disposedSubject.IsDisposed);
    }

    [Fact]
    public void LengthThrowsIfDisposed() {
        DataValue disposedSubject;

        using (var subject = new DataValue()) {
            disposedSubject = subject;
        };

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Length);
    }

    [Fact]
    public void GetValueThrowsIfDisposed() {
        DataValue disposedSubject;

        using (var subject = new DataValue()) {
            disposedSubject = subject;
        };

        Assert.Throws<ObjectDisposedException>(() => { var _ = disposedSubject.Value; });
    }

    [Fact]
    public void SetValueThrowsIfDisposed() {
        DataValue disposedSubject;

        using (var subject = new DataValue()) {
            disposedSubject = subject;
        };

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Value = new ReadOnlySpan<byte>([15, 15, 15]));
    }
}
