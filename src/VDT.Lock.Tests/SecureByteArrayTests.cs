using Xunit;

namespace VDT.Lock.Tests;

public sealed class SecureByteArrayTests {
    [Fact]
    public void Constructor() {
        using var subject = new SecureByteArray(16);

        Assert.Equal(16, subject.Buffer.Length);
    }

    [Fact]
    public void Clear() {
        using var subject = new SecureByteArray(16);

        for (var i = 0; i < 16; i++) {
            subject.Buffer[i] = 255;
        }

        subject.Clear();

        Assert.Equal(new byte[16], subject.Buffer);
    }

    [Fact]
    public void Dispose() {
        byte[] buffer;

        using (var subject = new SecureByteArray(16)) {
            for (var i = 0; i < 16; i++) {
                subject.Buffer[i] = 255;
            }

            buffer = subject.Buffer;
        }

        Assert.Equal(new byte[16], buffer);
    }
}
