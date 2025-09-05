using System;
using Xunit;

namespace VDT.Lock.Tests;

public class SecureBufferTests {
    [Fact]
    public void Length() {
        using var subject = new SecureBuffer([97, 98, 99]);

        Assert.Equal(subject.Value.Length, subject.Length);
    }

    [Fact]
    public void SerializeTo() {
        using var subject = new SecureBuffer([97, 98, 99]);

        using var result = new SecureByteList();
        subject.SerializeTo(result);

        Assert.Equal(new ReadOnlySpan<byte>([3, 0, 0, 0, 97, 98, 99]), result.GetValue());
    }

    [Fact]
    public void Dispose() {
        byte[] disposedValue;
        SecureBuffer disposedSubject;

        using (var subject = new SecureBuffer([97, 98, 99])) {
            disposedSubject = subject;
            disposedValue = subject.Value;
        }

        Assert.Equal([0, 0, 0], disposedValue);
        Assert.True(disposedSubject.IsDisposed);
    }

    [Fact]
    public void IsDisposed() {
        SecureBuffer disposedSubject;

        using (var subject = new SecureBuffer([97, 98, 99])) {
            disposedSubject = subject;
        }

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
