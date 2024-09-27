using System;
using Xunit;

namespace VDT.Lock.Tests;

public class SecureBufferTests {
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

        Assert.Throws<ObjectDisposedException>(() => disposedSubject.Value);
    }
}
