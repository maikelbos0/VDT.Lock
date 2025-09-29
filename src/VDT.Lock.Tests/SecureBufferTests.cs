using System;
using Xunit;

namespace VDT.Lock.Tests;

public class SecureBufferTests {
    [Fact]
    public void OperatorReadOnlySpan() {
        using var subject = new SecureBuffer([97, 98, 99]);
        ReadOnlySpan<byte> result = subject;

        Assert.Equal(new ReadOnlySpan<byte>([97, 98, 99]), result);
    }

    [Fact]
    public void Dispose() {
        SecureBuffer subject;
        byte[] disposedValue;
        SecureBuffer disposedSubject;

        using (subject = new([97, 98, 99])) {
            disposedValue = subject.Value;
        }

        Assert.True(subject.IsDisposed);
        Assert.Equal([0, 0, 0], disposedValue);
        Assert.True(disposedSubject.IsDisposed);
    }

    [Fact]
    public void ValueThrowsIfDisposed() {
        SecureBuffer disposedSubject;

        using (var subject = new SecureBuffer([97, 98, 99])) {
            disposedSubject = subject;
        }

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Value);
    }
}
